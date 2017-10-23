#if false
using Bridge.Html5;

namespace PoorMansTSqlFormatterJS
{
    //disabled temporarily, used for debugging JS constructs in Bridge.Net
    class HTML5Demo
    {
        public static void Main()
        {
            var div = new HTMLDivElement();

            var input = new HTMLInputElement()
            {
                Id = "number",
                Type = InputType.Text,
                Placeholder = "Enter a number to store…",
                Style =
        {
            Margin = "5px"
        }
            };

            var buttonSave = new HTMLButtonElement()
            {
                Id = "b",
                InnerHTML = "Save"
            };

            var buttonRestore = new HTMLButtonElement()
            {
                Id = "r",
                InnerHTML = "Restore",
                Style =
        {
            Margin = "5px"
        }
            };

            div.AppendChild(input);
            div.AppendChild(buttonSave);
            div.AppendChild(buttonRestore);

            var tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
            var stuff = tokenizer.TokenizeSQL("test me");

            Document.Body.AppendChild(div);
        }
    }
}
#endif
