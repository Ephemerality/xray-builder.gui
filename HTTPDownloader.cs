﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace XRayBuilderGUI
{
    // Taken from http://stackoverflow.com/a/2700707
    // Downloads an HTML page using appropriate encoding
    public class HttpDownloader
    {
        private readonly string _referer;
        private readonly string _userAgent;
        private readonly CookieContainer _cookiejar = new CookieContainer();
        private bool encodingFoundInHeader = false;

        public Encoding Encoding { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public Uri Url { get; set; }

        public static string GetPageHtml(string url)
        {
            HttpDownloader http = new HttpDownloader(url);
            return http.GetPage();
        }

        public HttpDownloader(string url) : this(url, null, null, null) { }

        public HttpDownloader(string url, CookieContainer jar, string referer, string userAgent)
        {
            Encoding = Encoding.GetEncoding("ISO-8859-1");
            Url = new Uri(url); // verify the uri
            _userAgent = userAgent;
            _referer = referer;
            if (jar != null) _cookiejar = jar;
        }

        public string GetPage()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (!string.IsNullOrEmpty(_referer))
                request.Referer = _referer;
            if (!string.IsNullOrEmpty(_userAgent))
                request.UserAgent = _userAgent;

            request.CookieContainer = _cookiejar;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Headers = response.Headers;
                Url = response.ResponseUri;
                return ProcessContent(response);
            }

        }

        private string ProcessContent(HttpWebResponse response)
        {
            SetEncodingFromHeader(response);

            Stream s = response.GetResponseStream();
            if (response.ContentEncoding.ToLower().Contains("gzip"))
                s = new GZipStream(s, CompressionMode.Decompress);
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
                s = new DeflateStream(s, CompressionMode.Decompress);

            MemoryStream memStream = new MemoryStream();
            int bytesRead;
            byte[] buffer = new byte[0x1000];
            for (bytesRead = s.Read(buffer, 0, buffer.Length); bytesRead > 0; bytesRead = s.Read(buffer, 0, buffer.Length))
            {
                memStream.Write(buffer, 0, bytesRead);
            }
            s.Close();
            string html;
            memStream.Position = 0;
            using (StreamReader r = new StreamReader(memStream, Encoding))
            {
                html = r.ReadToEnd().Trim();
                if (!encodingFoundInHeader)
                    html = CheckMetaCharSetAndReEncode(memStream, html);
            }

            return html;
        }

        private void SetEncodingFromHeader(HttpWebResponse response)
        {
            string charset = null;
            if (string.IsNullOrEmpty(response.CharacterSet))
            {
                Match m = Regex.Match(response.ContentType, @";\s*charset\s*=\s*(?<charset>.*)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    charset = m.Groups["charset"].Value.Trim(new[] { '\'', '"' });
                    encodingFoundInHeader = true;
                }
            }
            else
            {
                charset = response.CharacterSet;
            }
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    Encoding = Encoding.GetEncoding(charset);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        private string CheckMetaCharSetAndReEncode(Stream memStream, string html)
        {
            Match m = new Regex(@"<meta\s+.*?charset\s*=\s*(?<charset>[A-Za-z0-9_-]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Match(html);
            if (m.Success)
            {
                string charset = m.Groups["charset"].Value.ToLower() ?? "iso-8859-1";
                if ((charset == "unicode") || (charset == "utf-16"))
                {
                    charset = "utf-8";
                }

                try
                {
                    Encoding metaEncoding = Encoding.GetEncoding(charset);
                    if (Encoding != metaEncoding)
                    {
                        memStream.Position = 0L;
                        StreamReader recodeReader = new StreamReader(memStream, metaEncoding);
                        html = recodeReader.ReadToEnd().Trim();
                        recodeReader.Close();
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            return html;
        }
    }
}
