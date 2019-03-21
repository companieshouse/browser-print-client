namespace ChipsPrint
{
    class Options
    {
        private bool cert;
        private string url;
        private bool debug;

        public bool Cert { get => cert; set => cert = value; }
        public string Url { get => url; set => url = value; }
        public bool Debug { get => debug; set => debug = value; }
    }
}
