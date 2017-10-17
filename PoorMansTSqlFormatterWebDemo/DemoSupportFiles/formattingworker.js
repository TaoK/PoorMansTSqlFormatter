var tokenizer;
var parser;
var textFormatter;
var pageFormatter;

self.addEventListener('message', function (e) {
  var data = e.data;

  switch (data.commandToRun) {
    case 'init':
      self.postMessage(Startup(data.urlPrefix, data.scriptURLs));
      break;
    case 'format':
      self.postMessage(Format(data.paramLength, data.options, data.inputSql));
      break;
    default:
      HandleError('Unknown command: ' + data.commandToRun);
  }
}, false);

function HandleError(errorMessage) {
  var result = { status: 'failed', message: errorMessage };
  self.postMessage(result);
}

function Startup(urlPrefix, scriptURLs) {
  //Some error handling might be nice...
  for(var i = 0, len = scriptURLs.length; i < len; i++) {
    importScripts(urlPrefix + scriptURLs[i]);
  }

  tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
  parser = new PoorMansTSqlFormatterLib.Parsers.TSqlStandardParser();

  return {
    status: 'initialized'
  }
}

function Format(paramLength, options, inputSql) {
  //Some error handling might be nice...

  var textFormatter = PageFormattingMapper.GetFormatterForOptions(options, false, HandleError);
  var pageFormatter = new PoorMansTSqlFormatterLib.Formatters.HtmlPageWrapper(PageFormattingMapper.GetFormatterForOptions(options, true, HandleError));

  var tokenizedData = tokenizer.TokenizeSQL(inputSql);
  var parsedData = parser.ParseSQL(tokenizedData);

  var outputSqlText = textFormatter.FormatSQLTree(parsedData);
  var outputSqlHtml = pageFormatter.FormatSQLTree(parsedData);

  return {
      status: 'formatted',
      paramLength: paramLength,
      options: options,
      inputSql: inputSql,
      outputSqlText: outputSqlText,
      outputSqlHtml: outputSqlHtml
  };
}