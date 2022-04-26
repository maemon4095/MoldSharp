namespace MoldSharp;

struct GenerationOptions
    {
        public GenerationOptions()
        {
            this.ContextVariable = "context";
            this.ContextSource = "GetTransformContext";
            this.ExportMethod = "ToString";
            this.TransformTextMethod = "TransformText";
            this.TransformTextMethodParams = string.Empty;
            this.ReturnTextType = "string";
            this.TransformTextAccessibility = "public";
        }

        public string ContextVariable { get; set; }
        public bool ExternalContext => this.ContextSource is null;
        public string? ContextSource { get; set; }
        public string ExportMethod { get; set; }
        public string TransformTextMethod { get; set; }
        public string TransformTextMethodParams { get; set; }
        public string ReturnTextType { get; set; }
        public string TransformTextAccessibility { get; set; }
    }
