var Utils = function() {
  var ParseQueryString = function(input) {
    var output = {};
    var ampArray = (input[0] === '?' ? input.substr(1) : input).split('&');
    for (var i = 0; i < ampArray.length; i++) {
      var keyValuePair = ampArray[i].split('=');
      output[decodeURIComponent(keyValuePair[0].replace(/\+/g, ' '))] = decodeURIComponent((keyValuePair[1] || '').replace(/\+/g, ' '));
    }
    return output;
  }

  return { ParseQueryString: ParseQueryString }
}();

var JsFormattingEngine = function () {
  var scriptPrefix;
  var workerURL;
  var scriptURLs;

  var effectiveService = null;

  var initializationCompletionHandler;
  var generalErrorHandler;
  var resultSettingHandler;

  var LoadEnvironment = function (JSURLPrefix, workerScriptURL, pageRelativeScriptURLs, finishLoadGood, finishFormatGood, finishBad) {
    scriptPrefix = JSURLPrefix;
    workerURL = workerScriptURL;
    scriptURLs = pageRelativeScriptURLs;
    initializationCompletionHandler = finishLoadGood;
    resultSettingHandler = finishFormatGood;
    generalErrorHandler = finishBad;

    try {
      _WorkerService.Initialize();
    }
    catch (e) {
      console.log("Looks like workers are not available, let's load the formatting code in the main window directly.");
      _InPageService.Initialize()
    }
  }

  var _WorkerService = function () {
    var Initialize = function () {
      //throw "Test without workers";
      
      worker = new Worker(workerURL);
      
      worker.addEventListener('message', WorkerMessageHandler, false);
      
      worker.postMessage({
        commandToRun: "init",
        urlPrefix: scriptPrefix,
        scriptURLs: scriptURLs
      });
    }
    
    var WorkerMessageHandler = function (e) {
      var data = e.data;
      
      switch (data.status) {
        case 'initialized':
          effectiveService = _WorkerService;
          initializationCompletionHandler();
          break;
        case 'formatted':
          resultSettingHandler(data.paramLength, data.outputSqlHtml);
          break;
        default:
          generalErrorHandler('Invalid response from the formatting worker: ' + JSON.stringify(e.data));
      }
    }

    var RequestFormatting = function (initializationData) {
      worker.postMessage({ commandToRun: "format", paramLength: initializationData.paramLength, options: initializationData.options, inputSql: initializationData.inputSql });
    }
    
    return {
      Initialize: Initialize,
      RequestFormatting: RequestFormatting
    };
  }();

  var _InPageService = function() {
    var tokenizer;
    var parser;
    var textFormatter;
    var pageFormatter;

    //From https://www.nczonline.net/blog/2009/07/28/the-best-way-to-load-external-javascript/, adapted
    function _loadScript(url, successHandler, failureHandler, failureDelay) {

      var scriptLoadCompleted = false;

      var scriptElement = document.createElement("script")
      scriptElement.type = "text/javascript";
      scriptElement.src = url;
    

      var successWrapperHandler = function() {
        scriptLoadCompleted = true;
        successHandler();
      }
    
      var IEWrapperHandler = function() {
        if (scriptElement.readyState == "loaded" || scriptElement.readyState == "complete") {
          scriptElement.onreadystatechange = null;
          successWrapperHandler();
        }
      }
    
      if (scriptElement.readyState)
        scriptElement.onreadystatechange = IEWrapperHandler;
      else
        scriptElement.onload = successWrapperHandler;

      var failureWrapperHandler = function() {
        if (!scriptLoadCompleted)
          failureHandler();
      }
    
      document.getElementsByTagName("head")[0].appendChild(scriptElement);

      //default to 10 seconds for failure delay
      setTimeout(failureWrapperHandler, failureDelay || 10000);
    }

    var Initialize = function () {
      _LoadScriptsRecursive(0);
    }

    var _LoadScriptsRecursive = function (startIndex) {
      var scriptLoadedHandler = function() {
        console.log("Loaded " + scriptURLs[startIndex]);
        if (startIndex + 1 < scriptURLs.length)
          _LoadScriptsRecursive(startIndex + 1);
        else {
          tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
          parser = new PoorMansTSqlFormatterLib.Parsers.TSqlStandardParser();
          effectiveService = _InPageService;
          initializationCompletionHandler();
        }
      }
    
      var scriptLoadTimedOutHandler = function() {
        generalErrorHandler("Loading of formatting script timed out: " + scriptURLs[startIndex]);
      }
    
      _loadScript(scriptURLs[startIndex], scriptLoadedHandler, scriptLoadTimedOutHandler);
    }

    var _PrepSettings = function (options) {
      textFormatter = PageFormattingMapper.GetFormatterForOptions(options, false, generalErrorHandler);
      pageFormatter = new PoorMansTSqlFormatterLib.Formatters.HtmlPageWrapper(PageFormattingMapper.GetFormatterForOptions(options, true, generalErrorHandler));
    }


    var RequestFormatting = function (initializationData) {

      _PrepSettings(initializationData.options);

      var tokenizedData = tokenizer.TokenizeSQL(initializationData.inputSql);
      var parsedData = parser.ParseSQL(tokenizedData);

      var outputSqlText = textFormatter.FormatSQLTree(parsedData);
      var outputSqlHtml = pageFormatter.FormatSQLTree(parsedData);

      resultSettingHandler(initializationData.paramLength, outputSqlHtml);
    }

    return {
      Initialize: Initialize,
      RequestFormatting: RequestFormatting
    }
  }();

  var RequestFormatting = function(parameterString) {
/* Expect parms of the form:
formattingType=standard
indent=%5Ct
spacesPerTab=4
maxLineWidth=999
statementBreaks=2
clauseBreaks=1
expandCommaLists=true
trailingCommas=false
spaceAfterExpandedComma=false
expandBooleanExpressions=true
expandCaseStatements=true
expandBetweenConditions=true
expandInLists=true
breakJoinOnSections=false
uppercaseKeywords=true
coloring=true
keywordStandardization=true
randomizeColor=false
randomizeLineLengths=false
randomizeKeywordCase=false
preserveComments=false
enableKeywordSubstitution=false

(implemented in formattingmapper.js)
*/
    var parsedParams = Utils.ParseQueryString(parameterString);
    effectiveService.RequestFormatting({ paramLength: parameterString.length, options: parsedParams, inputSql: parsedParams.inputString });
  }

  return {
    LoadEnvironment: LoadEnvironment,
    RequestFormatting: RequestFormatting
  }
}();
