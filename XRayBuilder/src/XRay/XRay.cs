/*  Builds an X-Ray file to be used on the Amazon Kindle
*   Original xray builder by shinew, http://www.mobileread.com/forums/showthread.php?t=157770 , http://www.xunwang.me/xray/
*
*   Copyright (C) 2014 Ephemerality <Nick Niemi - ephemeral.vilification@gmail.com>
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.

*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.

*   You should have received a copy of the GNU General Public License
*   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.DataSources.Secondary.Model;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.XRay.Artifacts;
using XRayBuilderGUI.XRay.Logic.Chapters;
using XRayBuilderGUI.XRay.Model;

namespace XRayBuilderGUI.XRay
{
    public class XRay
    {
        private readonly ILogger _logger;

        public string DataUrl = "";
        private string xmlFile = "";
        public readonly string databaseName = "";
        private string _guid = "";
        public string Asin = "";
        private string _aliasPath;
        public List<Term> Terms = new List<Term>(100);
        public List<Chapter> Chapters = new List<Chapter>();
        public List<Excerpt> Excerpts = new List<Excerpt>();
        public long Srl;
        public long Erl;
        public bool SkipShelfari;
        public bool Unattended { get; set; }
        public readonly int LocOffset;
        public List<NotableClip> NotableClips;
        public int FoundNotables;
        public DateTime? CreatedAt { get; set; }

        public readonly ISecondarySource DataSource;
        private readonly ChaptersService _chaptersService;

        public delegate DialogResult SafeShowDelegate(string msg, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton def);

        public XRay(string shelfari, ISecondarySource dataSource, ILogger logger, ChaptersService chaptersService)
        {
            if (!shelfari.ToLower().StartsWith("http://") && !shelfari.ToLower().StartsWith("https://"))
                shelfari = "https://" + shelfari;
            DataUrl = shelfari;
            DataSource = dataSource;
            _logger = logger;
            _chaptersService = chaptersService;
        }

        public XRay(string shelfari, string db, string guid, string asin, ISecondarySource dataSource, ILogger logger, ChaptersService chaptersService, int locOffset = 0, string aliaspath = "")
        {
            if (shelfari == "" || db == "" || guid == "" || asin == "")
                throw new ArgumentException("Error initializing X-Ray, one of the required parameters was blank.");

            if (!shelfari.ToLower().StartsWith("http://") && !shelfari.ToLower().StartsWith("https://"))
                shelfari = "https://" + shelfari;
            DataUrl = shelfari;
            databaseName = db;
            if (guid != null)
                Guid = guid;
            Asin = asin;
            this.LocOffset = locOffset;
            _aliasPath = aliaspath;
            DataSource = dataSource;
            _logger = logger;
            _chaptersService = chaptersService;
        }

        // TODO fix this constructor crap
        public XRay(string xml, string db, string guid, string asin, ISecondarySource dataSource, ILogger logger, ChaptersService chaptersService, bool xmlUgh, int locOffset = 0, string aliaspath = "")
        {
            if (xml == "" || db == "" || guid == "" || asin == "")
                throw new ArgumentException("Error initializing X-Ray, one of the required parameters was blank.");
            xmlFile = xml;
            databaseName = db;
            Guid = guid;
            Asin = asin;
            this.LocOffset = locOffset;
            _aliasPath = aliaspath;
            DataSource = dataSource;
            _logger = logger;
            _chaptersService = chaptersService;
            SkipShelfari = true;
        }

        public string AliasPath
        {
            set => _aliasPath = value;
            // TODO directory service to handle default paths
            get => string.IsNullOrEmpty(_aliasPath) ? Environment.CurrentDirectory + @"\ext\" + Asin + ".aliases" : _aliasPath;
        }

        public string Guid
        {
            private set => _guid = Functions.ConvertGuid(value);
            get => _guid;
        }

        public string XRayName(bool android = false) =>
            android
                ? $"XRAY.{Asin}.{(databaseName == null ? "" : $"{databaseName}_")}{Guid ?? ""}.db"
                : $"XRAY.entities.{Asin}.asc";
    }
}
