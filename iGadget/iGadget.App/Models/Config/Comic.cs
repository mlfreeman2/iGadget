using System;



namespace iGadget.App.Models.Config 
{
    public class Comic 
    {
        public string Name { get; set; }

        public Uri URL { get; set; }

        public string Selector { get; set; }

        public string Before { get; set; }

        public string After { get; set; }

        public string Schedule { get; set; }
    }
}