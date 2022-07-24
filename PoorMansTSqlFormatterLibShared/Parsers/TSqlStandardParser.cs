﻿/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;
using System.Text;
using System.Text.RegularExpressions;

namespace PoorMansTSqlFormatterLib.Parsers
{
    public class TSqlStandardParser : ISqlTokenParser
    {
        /*
         * TODO:
         *  - handle Ranking Functions with multiple partition or order by columns/clauses
         *  - detect table hints, to avoid them looking like function parens
         *  - Handle DDL triggers
         *  - Detect ALTER keywords that are clauses of other statements, vs those that are statements
         *  
         *  - Tests
         *    - Samples illustrating all the tokens and container combinations implemented
         *    - Samples illustrating all forms of container violations
         *    - Sample requests and their XML equivalent - once the xml format is more-or-less formalized
         *    - Sample requests and their formatted versions (a few for each) - once the "standard" format is more-or-less formalized
         */

        //yay for static constructors!
        public static Dictionary<string, KeywordType> KeywordList { get; set; } = null!; // property always set in constructor
        static TSqlStandardParser()
        {
            InitializeKeywordList();
        }

        static Regex _JoinDetector = new Regex("^((RIGHT|INNER|LEFT|CROSS|FULL) )?(OUTER )?((HASH|LOOP|MERGE|REMOTE) )?(JOIN|APPLY) ");

        // original: static Regex _CursorDetector = new Regex(@"^DECLARE [\p{L}0-9_\$\@\#]+ ((INSENSITIVE|SCROLL) ){0,2}CURSOR "); //note the use of "unicode letter" in identifier rule
        // problem was that Bridge.Net innocently passes the "\p{L}" to JS, which does not understand (ES5) or refuses to understand (ES6)
        // ES6 unicode equivalency retrieved from ES6-to-ES5 regex unicode transpiler "regexpu" demo site https://mothereff.in/regexpu
        // (as far as I can tell the ES5 regex constructs used are also .Net-compatible)
        static Regex _CursorDetector = new Regex(@"^DECLARE (?:[#\$0-9@-Z_a-z\xAA\xB5\xBA\xC0-\xD6\xD8-\xF6\xF8-\u02C1\u02C6-\u02D1\u02E0-\u02E4\u02EC\u02EE\u0370-\u0374\u0376\u0377\u037A-\u037D\u037F\u0386\u0388-\u038A\u038C\u038E-\u03A1\u03A3-\u03F5\u03F7-\u0481\u048A-\u052F\u0531-\u0556\u0559\u0561-\u0587\u05D0-\u05EA\u05F0-\u05F2\u0620-\u064A\u066E\u066F\u0671-\u06D3\u06D5\u06E5\u06E6\u06EE\u06EF\u06FA-\u06FC\u06FF\u0710\u0712-\u072F\u074D-\u07A5\u07B1\u07CA-\u07EA\u07F4\u07F5\u07FA\u0800-\u0815\u081A\u0824\u0828\u0840-\u0858\u0860-\u086A\u08A0-\u08B4\u08B6-\u08BD\u0904-\u0939\u093D\u0950\u0958-\u0961\u0971-\u0980\u0985-\u098C\u098F\u0990\u0993-\u09A8\u09AA-\u09B0\u09B2\u09B6-\u09B9\u09BD\u09CE\u09DC\u09DD\u09DF-\u09E1\u09F0\u09F1\u09FC\u0A05-\u0A0A\u0A0F\u0A10\u0A13-\u0A28\u0A2A-\u0A30\u0A32\u0A33\u0A35\u0A36\u0A38\u0A39\u0A59-\u0A5C\u0A5E\u0A72-\u0A74\u0A85-\u0A8D\u0A8F-\u0A91\u0A93-\u0AA8\u0AAA-\u0AB0\u0AB2\u0AB3\u0AB5-\u0AB9\u0ABD\u0AD0\u0AE0\u0AE1\u0AF9\u0B05-\u0B0C\u0B0F\u0B10\u0B13-\u0B28\u0B2A-\u0B30\u0B32\u0B33\u0B35-\u0B39\u0B3D\u0B5C\u0B5D\u0B5F-\u0B61\u0B71\u0B83\u0B85-\u0B8A\u0B8E-\u0B90\u0B92-\u0B95\u0B99\u0B9A\u0B9C\u0B9E\u0B9F\u0BA3\u0BA4\u0BA8-\u0BAA\u0BAE-\u0BB9\u0BD0\u0C05-\u0C0C\u0C0E-\u0C10\u0C12-\u0C28\u0C2A-\u0C39\u0C3D\u0C58-\u0C5A\u0C60\u0C61\u0C80\u0C85-\u0C8C\u0C8E-\u0C90\u0C92-\u0CA8\u0CAA-\u0CB3\u0CB5-\u0CB9\u0CBD\u0CDE\u0CE0\u0CE1\u0CF1\u0CF2\u0D05-\u0D0C\u0D0E-\u0D10\u0D12-\u0D3A\u0D3D\u0D4E\u0D54-\u0D56\u0D5F-\u0D61\u0D7A-\u0D7F\u0D85-\u0D96\u0D9A-\u0DB1\u0DB3-\u0DBB\u0DBD\u0DC0-\u0DC6\u0E01-\u0E30\u0E32\u0E33\u0E40-\u0E46\u0E81\u0E82\u0E84\u0E87\u0E88\u0E8A\u0E8D\u0E94-\u0E97\u0E99-\u0E9F\u0EA1-\u0EA3\u0EA5\u0EA7\u0EAA\u0EAB\u0EAD-\u0EB0\u0EB2\u0EB3\u0EBD\u0EC0-\u0EC4\u0EC6\u0EDC-\u0EDF\u0F00\u0F40-\u0F47\u0F49-\u0F6C\u0F88-\u0F8C\u1000-\u102A\u103F\u1050-\u1055\u105A-\u105D\u1061\u1065\u1066\u106E-\u1070\u1075-\u1081\u108E\u10A0-\u10C5\u10C7\u10CD\u10D0-\u10FA\u10FC-\u1248\u124A-\u124D\u1250-\u1256\u1258\u125A-\u125D\u1260-\u1288\u128A-\u128D\u1290-\u12B0\u12B2-\u12B5\u12B8-\u12BE\u12C0\u12C2-\u12C5\u12C8-\u12D6\u12D8-\u1310\u1312-\u1315\u1318-\u135A\u1380-\u138F\u13A0-\u13F5\u13F8-\u13FD\u1401-\u166C\u166F-\u167F\u1681-\u169A\u16A0-\u16EA\u16F1-\u16F8\u1700-\u170C\u170E-\u1711\u1720-\u1731\u1740-\u1751\u1760-\u176C\u176E-\u1770\u1780-\u17B3\u17D7\u17DC\u1820-\u1877\u1880-\u1884\u1887-\u18A8\u18AA\u18B0-\u18F5\u1900-\u191E\u1950-\u196D\u1970-\u1974\u1980-\u19AB\u19B0-\u19C9\u1A00-\u1A16\u1A20-\u1A54\u1AA7\u1B05-\u1B33\u1B45-\u1B4B\u1B83-\u1BA0\u1BAE\u1BAF\u1BBA-\u1BE5\u1C00-\u1C23\u1C4D-\u1C4F\u1C5A-\u1C7D\u1C80-\u1C88\u1CE9-\u1CEC\u1CEE-\u1CF1\u1CF5\u1CF6\u1D00-\u1DBF\u1E00-\u1F15\u1F18-\u1F1D\u1F20-\u1F45\u1F48-\u1F4D\u1F50-\u1F57\u1F59\u1F5B\u1F5D\u1F5F-\u1F7D\u1F80-\u1FB4\u1FB6-\u1FBC\u1FBE\u1FC2-\u1FC4\u1FC6-\u1FCC\u1FD0-\u1FD3\u1FD6-\u1FDB\u1FE0-\u1FEC\u1FF2-\u1FF4\u1FF6-\u1FFC\u2071\u207F\u2090-\u209C\u2102\u2107\u210A-\u2113\u2115\u2119-\u211D\u2124\u2126\u2128\u212A-\u212D\u212F-\u2139\u213C-\u213F\u2145-\u2149\u214E\u2183\u2184\u2C00-\u2C2E\u2C30-\u2C5E\u2C60-\u2CE4\u2CEB-\u2CEE\u2CF2\u2CF3\u2D00-\u2D25\u2D27\u2D2D\u2D30-\u2D67\u2D6F\u2D80-\u2D96\u2DA0-\u2DA6\u2DA8-\u2DAE\u2DB0-\u2DB6\u2DB8-\u2DBE\u2DC0-\u2DC6\u2DC8-\u2DCE\u2DD0-\u2DD6\u2DD8-\u2DDE\u2E2F\u3005\u3006\u3031-\u3035\u303B\u303C\u3041-\u3096\u309D-\u309F\u30A1-\u30FA\u30FC-\u30FF\u3105-\u312E\u3131-\u318E\u31A0-\u31BA\u31F0-\u31FF\u3400-\u4DB5\u4E00-\u9FEA\uA000-\uA48C\uA4D0-\uA4FD\uA500-\uA60C\uA610-\uA61F\uA62A\uA62B\uA640-\uA66E\uA67F-\uA69D\uA6A0-\uA6E5\uA717-\uA71F\uA722-\uA788\uA78B-\uA7AE\uA7B0-\uA7B7\uA7F7-\uA801\uA803-\uA805\uA807-\uA80A\uA80C-\uA822\uA840-\uA873\uA882-\uA8B3\uA8F2-\uA8F7\uA8FB\uA8FD\uA90A-\uA925\uA930-\uA946\uA960-\uA97C\uA984-\uA9B2\uA9CF\uA9E0-\uA9E4\uA9E6-\uA9EF\uA9FA-\uA9FE\uAA00-\uAA28\uAA40-\uAA42\uAA44-\uAA4B\uAA60-\uAA76\uAA7A\uAA7E-\uAAAF\uAAB1\uAAB5\uAAB6\uAAB9-\uAABD\uAAC0\uAAC2\uAADB-\uAADD\uAAE0-\uAAEA\uAAF2-\uAAF4\uAB01-\uAB06\uAB09-\uAB0E\uAB11-\uAB16\uAB20-\uAB26\uAB28-\uAB2E\uAB30-\uAB5A\uAB5C-\uAB65\uAB70-\uABE2\uAC00-\uD7A3\uD7B0-\uD7C6\uD7CB-\uD7FB\uF900-\uFA6D\uFA70-\uFAD9\uFB00-\uFB06\uFB13-\uFB17\uFB1D\uFB1F-\uFB28\uFB2A-\uFB36\uFB38-\uFB3C\uFB3E\uFB40\uFB41\uFB43\uFB44\uFB46-\uFBB1\uFBD3-\uFD3D\uFD50-\uFD8F\uFD92-\uFDC7\uFDF0-\uFDFB\uFE70-\uFE74\uFE76-\uFEFC\uFF21-\uFF3A\uFF41-\uFF5A\uFF66-\uFFBE\uFFC2-\uFFC7\uFFCA-\uFFCF\uFFD2-\uFFD7\uFFDA-\uFFDC]|\uD800[\uDC00-\uDC0B\uDC0D-\uDC26\uDC28-\uDC3A\uDC3C\uDC3D\uDC3F-\uDC4D\uDC50-\uDC5D\uDC80-\uDCFA\uDE80-\uDE9C\uDEA0-\uDED0\uDF00-\uDF1F\uDF2D-\uDF40\uDF42-\uDF49\uDF50-\uDF75\uDF80-\uDF9D\uDFA0-\uDFC3\uDFC8-\uDFCF]|\uD801[\uDC00-\uDC9D\uDCB0-\uDCD3\uDCD8-\uDCFB\uDD00-\uDD27\uDD30-\uDD63\uDE00-\uDF36\uDF40-\uDF55\uDF60-\uDF67]|\uD802[\uDC00-\uDC05\uDC08\uDC0A-\uDC35\uDC37\uDC38\uDC3C\uDC3F-\uDC55\uDC60-\uDC76\uDC80-\uDC9E\uDCE0-\uDCF2\uDCF4\uDCF5\uDD00-\uDD15\uDD20-\uDD39\uDD80-\uDDB7\uDDBE\uDDBF\uDE00\uDE10-\uDE13\uDE15-\uDE17\uDE19-\uDE33\uDE60-\uDE7C\uDE80-\uDE9C\uDEC0-\uDEC7\uDEC9-\uDEE4\uDF00-\uDF35\uDF40-\uDF55\uDF60-\uDF72\uDF80-\uDF91]|\uD803[\uDC00-\uDC48\uDC80-\uDCB2\uDCC0-\uDCF2]|\uD804[\uDC03-\uDC37\uDC83-\uDCAF\uDCD0-\uDCE8\uDD03-\uDD26\uDD50-\uDD72\uDD76\uDD83-\uDDB2\uDDC1-\uDDC4\uDDDA\uDDDC\uDE00-\uDE11\uDE13-\uDE2B\uDE80-\uDE86\uDE88\uDE8A-\uDE8D\uDE8F-\uDE9D\uDE9F-\uDEA8\uDEB0-\uDEDE\uDF05-\uDF0C\uDF0F\uDF10\uDF13-\uDF28\uDF2A-\uDF30\uDF32\uDF33\uDF35-\uDF39\uDF3D\uDF50\uDF5D-\uDF61]|\uD805[\uDC00-\uDC34\uDC47-\uDC4A\uDC80-\uDCAF\uDCC4\uDCC5\uDCC7\uDD80-\uDDAE\uDDD8-\uDDDB\uDE00-\uDE2F\uDE44\uDE80-\uDEAA\uDF00-\uDF19]|\uD806[\uDCA0-\uDCDF\uDCFF\uDE00\uDE0B-\uDE32\uDE3A\uDE50\uDE5C-\uDE83\uDE86-\uDE89\uDEC0-\uDEF8]|\uD807[\uDC00-\uDC08\uDC0A-\uDC2E\uDC40\uDC72-\uDC8F\uDD00-\uDD06\uDD08\uDD09\uDD0B-\uDD30\uDD46]|\uD808[\uDC00-\uDF99]|\uD809[\uDC80-\uDD43]|[\uD80C\uD81C-\uD820\uD840-\uD868\uD86A-\uD86C\uD86F-\uD872\uD874-\uD879][\uDC00-\uDFFF]|\uD80D[\uDC00-\uDC2E]|\uD811[\uDC00-\uDE46]|\uD81A[\uDC00-\uDE38\uDE40-\uDE5E\uDED0-\uDEED\uDF00-\uDF2F\uDF40-\uDF43\uDF63-\uDF77\uDF7D-\uDF8F]|\uD81B[\uDF00-\uDF44\uDF50\uDF93-\uDF9F\uDFE0\uDFE1]|\uD821[\uDC00-\uDFEC]|\uD822[\uDC00-\uDEF2]|\uD82C[\uDC00-\uDD1E\uDD70-\uDEFB]|\uD82F[\uDC00-\uDC6A\uDC70-\uDC7C\uDC80-\uDC88\uDC90-\uDC99]|\uD835[\uDC00-\uDC54\uDC56-\uDC9C\uDC9E\uDC9F\uDCA2\uDCA5\uDCA6\uDCA9-\uDCAC\uDCAE-\uDCB9\uDCBB\uDCBD-\uDCC3\uDCC5-\uDD05\uDD07-\uDD0A\uDD0D-\uDD14\uDD16-\uDD1C\uDD1E-\uDD39\uDD3B-\uDD3E\uDD40-\uDD44\uDD46\uDD4A-\uDD50\uDD52-\uDEA5\uDEA8-\uDEC0\uDEC2-\uDEDA\uDEDC-\uDEFA\uDEFC-\uDF14\uDF16-\uDF34\uDF36-\uDF4E\uDF50-\uDF6E\uDF70-\uDF88\uDF8A-\uDFA8\uDFAA-\uDFC2\uDFC4-\uDFCB]|\uD83A[\uDC00-\uDCC4\uDD00-\uDD43]|\uD83B[\uDE00-\uDE03\uDE05-\uDE1F\uDE21\uDE22\uDE24\uDE27\uDE29-\uDE32\uDE34-\uDE37\uDE39\uDE3B\uDE42\uDE47\uDE49\uDE4B\uDE4D-\uDE4F\uDE51\uDE52\uDE54\uDE57\uDE59\uDE5B\uDE5D\uDE5F\uDE61\uDE62\uDE64\uDE67-\uDE6A\uDE6C-\uDE72\uDE74-\uDE77\uDE79-\uDE7C\uDE7E\uDE80-\uDE89\uDE8B-\uDE9B\uDEA1-\uDEA3\uDEA5-\uDEA9\uDEAB-\uDEBB]|\uD869[\uDC00-\uDED6\uDF00-\uDFFF]|\uD86D[\uDC00-\uDF34\uDF40-\uDFFF]|\uD86E[\uDC00-\uDC1D\uDC20-\uDFFF]|\uD873[\uDC00-\uDEA1\uDEB0-\uDFFF]|\uD87A[\uDC00-\uDFE0]|\uD87E[\uDC00-\uDE1D])+ ((INSENSITIVE|SCROLL) ){0,2}CURSOR "); //note the use of "unicode letter" in identifier rule
        static Regex _TriggerConditionDetector = new Regex(@"^(FOR|AFTER|INSTEAD OF)( (INSERT|UPDATE|DELETE) (, (INSERT|UPDATE|DELETE) )?(, (INSERT|UPDATE|DELETE) )?)"); //note the use of "unicode letter" in identifier rule

        public Node ParseSQL(ITokenList tokenList)
        {
            ParseTree sqlTree = new(SqlStructureConstants.ENAME_SQL_ROOT);
            sqlTree.StartNewStatement();

            int tokenCount = tokenList.Count;
            int tokenID = 0;
            while (tokenID < tokenCount)
            {
                IToken token = tokenList[tokenID];

                switch (token.Type)
                {
                    case SqlTokenType.OpenParens:
						Node? firstNonCommentParensSibling = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
						Node? lastNonCommentParensSibling = sqlTree.GetLastNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
						bool isInsertOrValuesClause = (
                            firstNonCommentParensSibling != null
                            && (
                                (string.Equals(firstNonCommentParensSibling.Name, SqlStructureConstants.ENAME_OTHERKEYWORD)
                                   && (firstNonCommentParensSibling.TextValue ?? "").ToUpperInvariant().StartsWith("INSERT")
                                   )
                                || 
                                (string.Equals(firstNonCommentParensSibling.Name, SqlStructureConstants.ENAME_COMPOUNDKEYWORD)
                                   && (firstNonCommentParensSibling.GetAttributeValue(SqlStructureConstants.ANAME_SIMPLETEXT) ?? "").ToUpperInvariant().StartsWith("INSERT ")
                                   )
                                ||
                                (string.Equals(firstNonCommentParensSibling.Name, SqlStructureConstants.ENAME_OTHERKEYWORD)
                                   && (firstNonCommentParensSibling.TextValue ?? "").ToUpperInvariant().StartsWith("VALUES")
                                   )
                               )
                            );

                        if (string.Equals(sqlTree.CurrentContainer.Name, SqlStructureConstants.ENAME_CTE_ALIAS)
                            && string.Equals(sqlTree.CurrentContainer.Parent?.Name, SqlStructureConstants.ENAME_CTE_WITH_CLAUSE)
                            )
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_DDL_PARENS, "");
                        else if (string.Equals(sqlTree.CurrentContainer.Name, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                            && string.Equals(sqlTree.CurrentContainer.Parent?.Name, SqlStructureConstants.ENAME_CTE_AS_BLOCK)
                            )
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS, "");
                        else if (firstNonCommentParensSibling == null
                            && string.Equals(sqlTree.CurrentContainer.Name, SqlStructureConstants.ENAME_SELECTIONTARGET)
                            )
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS, "");
                        else if (firstNonCommentParensSibling != null
                            && string.Equals(firstNonCommentParensSibling.Name, SqlStructureConstants.ENAME_SET_OPERATOR_CLAUSE)
                            )
                        {
                            sqlTree.ConsiderStartingNewClause();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS, "");
                        }
                        else if (IsLatestTokenADDLDetailValue(sqlTree))
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_DDLDETAIL_PARENS, "");
                        else if (string.Equals(sqlTree.CurrentContainer.Name, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                            || string.Equals(sqlTree.CurrentContainer.Name, SqlStructureConstants.ENAME_DDL_OTHER_BLOCK)
                            || string.Equals(sqlTree.CurrentContainer.Name, SqlStructureConstants.ENAME_DDL_DECLARE_BLOCK)
                            || (string.Equals(sqlTree.CurrentContainer.Name, SqlStructureConstants.ENAME_SQL_CLAUSE) 
                                && (firstNonCommentParensSibling != null
                                    && string.Equals(firstNonCommentParensSibling.Name, SqlStructureConstants.ENAME_OTHERKEYWORD)
                                    && (firstNonCommentParensSibling.TextValue ?? "").ToUpperInvariant().StartsWith("OPTION")
                                    )
                                )
                            || isInsertOrValuesClause
                            )
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_DDL_PARENS, "");
						else if ((lastNonCommentParensSibling != null
									&& string.Equals(lastNonCommentParensSibling.Name, SqlStructureConstants.ENAME_ALPHAOPERATOR)
									&& (lastNonCommentParensSibling.TextValue ?? "").ToUpperInvariant().Equals("IN")
									)
							)
							sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_IN_PARENS, "");
						else if (IsLatestTokenAMiscName(sqlTree))
							sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_FUNCTION_PARENS, "");
						else
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_EXPRESSION_PARENS, "");
                        break;

                    case SqlTokenType.CloseParens:
                        //we're not likely to actually have a "SingleStatement" in parens, but 
                        // we definitely want the side-effects (all the lower-level escapes)
                        sqlTree.EscapeAnySingleOrPartialStatementContainers();

                        //check whether we expected to end the parens...
                        if (sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_DDLDETAIL_PARENS)
                            || sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_DDL_PARENS)
							|| sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_FUNCTION_PARENS)
							|| sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_IN_PARENS)
							|| sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_EXPRESSION_PARENS)
                            || sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS)
                            )
                        {
                            sqlTree.MoveToAncestorContainer(1); //unspecified parent node...
                        }
                        else if (sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_SQL_CLAUSE)
                                && string.Equals(sqlTree.CurrentContainer.Parent?.Name, SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS)
                                && string.Equals(sqlTree.CurrentContainer.Parent?.Parent?.Name, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && string.Equals(sqlTree.CurrentContainer.Parent?.Parent?.Parent?.Name, SqlStructureConstants.ENAME_CTE_AS_BLOCK)
                                )
                        {
                            sqlTree.MoveToAncestorContainer(4, SqlStructureConstants.ENAME_CTE_WITH_CLAUSE);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT, "");
                        }
                        else if (sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_SQL_CLAUSE)
                                && (
                                    string.Equals(sqlTree.CurrentContainer.Parent?.Name, SqlStructureConstants.ENAME_EXPRESSION_PARENS)
									|| string.Equals(sqlTree.CurrentContainer.Parent?.Name, SqlStructureConstants.ENAME_IN_PARENS)
									|| string.Equals(sqlTree.CurrentContainer.Parent?.Name, SqlStructureConstants.ENAME_SELECTIONTARGET_PARENS)
                                )
                            )
                        {
                            sqlTree.MoveToAncestorContainer(2); //unspecified grandfather node.
                        }
                        else
                        {
                            sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHERNODE, ")");
                        }
                        break;

                    case SqlTokenType.OtherNode:

                        //prepare multi-keyword detection by "peeking" up to 7 keywords ahead
                        List<int> significantTokenPositions = GetSignificantTokenPositions(tokenList, tokenID, 7);
                        string significantTokensString = ExtractTokensString(tokenList, significantTokenPositions);

                        if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_PERMISSIONS_DETAIL))
                        {
                            //if we're in a permissions detail clause, we can expect all sorts of statements 
                            // starters and should ignore them all; the only possible keywords to escape are
                            // "ON" and "TO".
                            if (significantTokensString.StartsWith("ON "))
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_PERMISSIONS_TARGET, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (significantTokensString.StartsWith("TO ")
                                || significantTokensString.StartsWith("FROM ")
                                )
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_PERMISSIONS_RECIPIENT, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else 
                            {
                                //default to "some classification of permission"
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("CREATE PROC")
                            || significantTokensString.StartsWith("CREATE FUNC")
                            || significantTokensString.StartsWith("CREATE TRIGGER ")
                            || significantTokensString.StartsWith("CREATE VIEW ")
                            || significantTokensString.StartsWith("ALTER PROC")
                            || significantTokensString.StartsWith("ALTER FUNC")
                            || significantTokensString.StartsWith("ALTER TRIGGER ")
                            || significantTokensString.StartsWith("ALTER VIEW ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK, "");
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (_CursorDetector.IsMatch(significantTokensString))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CURSOR_DECLARATION, "");
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                            && _TriggerConditionDetector.IsMatch(significantTokensString)
                            )
                        {
                            //horrible complicated forward-search, to avoid having to keep a different "Trigger Condition" state for Update, Insert and Delete statement-starting keywords 
                            Match triggerConditions = _TriggerConditionDetector.Match(significantTokensString);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_TRIGGER_CONDITION, "");
                            Node triggerConditionType = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_COMPOUNDKEYWORD, "");

                            //first set the "trigger condition type": FOR, INSTEAD OF, AFTER
                            string triggerConditionTypeSimpleText = triggerConditions.Groups[1].Value;
                            triggerConditionType.SetAttribute(SqlStructureConstants.ANAME_SIMPLETEXT, triggerConditionTypeSimpleText);
                            int triggerConditionTypeNodeCount = triggerConditionTypeSimpleText.Split(new char[] { ' ' }).Length; //there's probably a better way of counting words...
                            AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[0], significantTokenPositions[triggerConditionTypeNodeCount - 1]), SqlStructureConstants.ENAME_OTHERKEYWORD, triggerConditionType);

                            //then get the count of conditions (INSERT, UPDATE, DELETE) and add those too...
                            int triggerConditionNodeCount = triggerConditions.Groups[2].Value.Split(new char[] { ' ' }).Length - 2; //there's probably a better way of counting words...
                            AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[triggerConditionTypeNodeCount - 1] + 1, significantTokenPositions[triggerConditionTypeNodeCount + triggerConditionNodeCount - 1]), SqlStructureConstants.ENAME_OTHERKEYWORD, sqlTree.CurrentContainer);
                            tokenID = significantTokenPositions[triggerConditionTypeNodeCount + triggerConditionNodeCount - 1];
                            sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK);
                        }
                        else if (significantTokensString.StartsWith("FOR "))
                        {
                            sqlTree.EscapeAnyBetweenConditions();
                            sqlTree.EscapeAnySelectionTarget();
                            sqlTree.EscapeJoinCondition();

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CURSOR_DECLARATION))
                            {
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_CURSOR_FOR_BLOCK, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                                sqlTree.StartNewStatement();
                            }
                            else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                                && sqlTree.PathNameMatches(2, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(3, SqlStructureConstants.ENAME_CURSOR_FOR_BLOCK)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(4, SqlStructureConstants.ENAME_CURSOR_DECLARATION);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_CURSOR_FOR_OPTIONS, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                //Assume FOR clause if we're at clause level
                                // (otherwise, eg in OPTIMIZE FOR UNKNOWN, this will just not do anything)
                                sqlTree.ConsiderStartingNewClause();

                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("DECLARE "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_DDL_DECLARE_BLOCK, "");
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("CREATE ")
                            || significantTokensString.StartsWith("ALTER ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_DDL_OTHER_BLOCK, "");
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("GRANT ")
                            || significantTokensString.StartsWith("DENY ")
                            || significantTokensString.StartsWith("REVOKE ")
                            )
                        {
                            if (significantTokensString.StartsWith("GRANT ")
                                && sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_DDL_WITH_CLAUSE)
                                && sqlTree.PathNameMatches(2, SqlStructureConstants.ENAME_PERMISSIONS_BLOCK)
                                && sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer) == null
                                )
                            {
                                //this MUST be a "WITH GRANT OPTION" option...
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                            else
                            {
                                sqlTree.ConsiderStartingNewStatement();
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_PERMISSIONS_BLOCK, token.Value, SqlStructureConstants.ENAME_PERMISSIONS_DETAIL);
                            }
                        }
                        else if (sqlTree.CurrentContainer.Name.Equals(SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                            && significantTokensString.StartsWith("RETURNS ")
                            )
                        {
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_DDL_RETURNS, ""));
                        }
                        else if (significantTokensString.StartsWith("AS "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK))
                            {
                                KeywordType nextKeywordType;
                                bool isDataTypeDefinition = false;
                                if (significantTokenPositions.Count > 1
                                    && KeywordList.TryGetValue(tokenList[significantTokenPositions[1]].Value.ToUpperInvariant(), out nextKeywordType)
                                    )
                                    if (nextKeywordType == KeywordType.DataTypeKeyword)
                                        isDataTypeDefinition = true;

                                if (isDataTypeDefinition)
                                {
                                    //this is actually a data type declaration (redundant "AS"...), save as regular token.
                                    sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                                }
                                else
                                {
                                    //this is the start of the object content definition
                                    sqlTree.StartNewContainer(SqlStructureConstants.ENAME_DDL_AS_BLOCK, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                                    sqlTree.StartNewStatement();
                                }
                            }
                            else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_DDL_WITH_CLAUSE)
                                && sqlTree.PathNameMatches(2, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_DDL_AS_BLOCK, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                                sqlTree.StartNewStatement();
                            }
                            else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CTE_ALIAS)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_CTE_WITH_CLAUSE)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_CTE_WITH_CLAUSE);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_CTE_AS_BLOCK, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("BEGIN DISTRIBUTED TRANSACTION ")
                            || significantTokensString.StartsWith("BEGIN DISTRIBUTED TRAN ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_BEGIN_TRANSACTION, ""), ref tokenID, significantTokenPositions, 3);
                        }
                        else if (significantTokensString.StartsWith("BEGIN TRANSACTION ")
                            || significantTokensString.StartsWith("BEGIN TRAN ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_BEGIN_TRANSACTION, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("SAVE TRANSACTION ")
                            || significantTokensString.StartsWith("SAVE TRAN ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SAVE_TRANSACTION, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("COMMIT TRANSACTION ")
                            || significantTokensString.StartsWith("COMMIT TRAN ")
                            || significantTokensString.StartsWith("COMMIT WORK ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_COMMIT_TRANSACTION, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("COMMIT "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_COMMIT_TRANSACTION, token.Value));
                        }
                        else if (significantTokensString.StartsWith("ROLLBACK TRANSACTION ")
                            || significantTokensString.StartsWith("ROLLBACK TRAN ")
                            || significantTokensString.StartsWith("ROLLBACK WORK ")
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_ROLLBACK_TRANSACTION, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("ROLLBACK "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_ROLLBACK_TRANSACTION, token.Value));
                        }
                        else if (significantTokensString.StartsWith("BEGIN TRY "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            Node newTryBlock = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_TRY_BLOCK, "");
                            Node tryContainerOpen = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_OPEN, "", newTryBlock);
                            ProcessCompoundKeyword(tokenList, sqlTree, tryContainerOpen, ref tokenID, significantTokenPositions, 2);
                            Node tryMultiContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_MULTISTATEMENT, "", newTryBlock);
                            sqlTree.StartNewStatement(tryMultiContainer);
                        }
                        else if (significantTokensString.StartsWith("BEGIN CATCH "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            Node newCatchBlock = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CATCH_BLOCK, "");
                            Node catchContainerOpen = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_OPEN, "", newCatchBlock);
                            ProcessCompoundKeyword(tokenList, sqlTree, catchContainerOpen, ref tokenID, significantTokenPositions, 2);
                            Node catchMultiContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_MULTISTATEMENT, "", newCatchBlock);
                            sqlTree.StartNewStatement(catchMultiContainer);
                        }
                        else if (significantTokensString.StartsWith("BEGIN "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.StartNewContainer(SqlStructureConstants.ENAME_BEGIN_END_BLOCK, token.Value, SqlStructureConstants.ENAME_CONTAINER_MULTISTATEMENT);
                            sqlTree.StartNewStatement();
                        }
                        else if (significantTokensString.StartsWith("MERGE "))
                        {
                            //According to BOL, MERGE is a fully reserved keyword from compat 100 onwards, for the MERGE statement only.
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.ConsiderStartingNewClause();
                            sqlTree.StartNewContainer(SqlStructureConstants.ENAME_MERGE_CLAUSE, token.Value, SqlStructureConstants.ENAME_MERGE_TARGET);
                        }
                        else if (significantTokensString.StartsWith("USING "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_MERGE_TARGET))
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_MERGE_CLAUSE);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_MERGE_USING, token.Value, SqlStructureConstants.ENAME_SELECTIONTARGET);
                            }
                            else
                                sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHERNODE, token.Value);
                        }
                        else if (significantTokensString.StartsWith("ON "))
                        {
                            sqlTree.EscapeAnySelectionTarget();

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_MERGE_USING))
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_MERGE_CLAUSE);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_MERGE_CONDITION, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (!sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                                && !sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_DDL_OTHER_BLOCK)
                                && !sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_DDL_WITH_CLAUSE)
                                && !sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_EXPRESSION_PARENS)
                                && !ContentStartsWithKeyword(sqlTree.CurrentContainer, "SET")
                                )
                            {
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_JOIN_ON_SECTION, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("CASE "))
                        {
                            sqlTree.StartNewContainer(SqlStructureConstants.ENAME_CASE_STATEMENT, token.Value, SqlStructureConstants.ENAME_CASE_INPUT);
                        }
                        else if (significantTokensString.StartsWith("WHEN "))
                        {
                            sqlTree.EscapeMergeAction();

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CASE_INPUT)
                                || (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                    && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_CASE_THEN)
                                    )
                                )
                            {
                                if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CASE_INPUT))
                                    sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_CASE_STATEMENT);
                                else
                                    sqlTree.MoveToAncestorContainer(3, SqlStructureConstants.ENAME_CASE_STATEMENT);

                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_CASE_WHEN, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if ((sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                    && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_MERGE_CONDITION)
                                    )
                                || sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_MERGE_WHEN)
                                )
                            {
                                if (sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_MERGE_CONDITION))
                                    sqlTree.MoveToAncestorContainer(2, SqlStructureConstants.ENAME_MERGE_CLAUSE);
                                else
                                    sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_MERGE_CLAUSE);

                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_MERGE_WHEN, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                                sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHERNODE, token.Value);
                        }
                        else if (significantTokensString.StartsWith("THEN "))
                        {
                            sqlTree.EscapeAnyBetweenConditions();

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_CASE_WHEN)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_CASE_WHEN);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_CASE_THEN, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_MERGE_WHEN)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_MERGE_WHEN);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_MERGE_THEN, token.Value, SqlStructureConstants.ENAME_MERGE_ACTION);
                                sqlTree.StartNewStatement();
                            }
                            else
                                sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHERNODE, token.Value);
                        }
                        else if (significantTokensString.StartsWith("OUTPUT "))
                        {
                            bool isSprocArgument = false;

                            //We're looking for sproc calls - they can't be nested inside anything else (as far as I know)
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                                && (ContentStartsWithKeyword(sqlTree.CurrentContainer, "EXEC")
                                    || ContentStartsWithKeyword(sqlTree.CurrentContainer, "EXECUTE")
                                    || ContentStartsWithKeyword(sqlTree.CurrentContainer, null)
                                    )
                                )
                            {
                                isSprocArgument = true;
                            }

                            //Also proc definitions - argument lists without parens
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK))
                                isSprocArgument = true;

                            if (!isSprocArgument)
                            {
                                sqlTree.EscapeMergeAction();
                                sqlTree.ConsiderStartingNewClause();
                            }

                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("OPTION "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_DDL_WITH_CLAUSE)
                                )
                            {
                                //"OPTION" keyword here is NOT indicative of a new clause.
                            }
                            else
                            {
                                sqlTree.EscapeMergeAction();
                                sqlTree.ConsiderStartingNewClause();
                            }
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("END TRY "))
                        {
                            sqlTree.EscapeAnySingleOrPartialStatementContainers();

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                                && sqlTree.PathNameMatches(2, SqlStructureConstants.ENAME_CONTAINER_MULTISTATEMENT)
                                && sqlTree.PathNameMatches(3, SqlStructureConstants.ENAME_TRY_BLOCK)
                                )
                            {
                                //clause.statement.multicontainer.try
                                Node? tryBlock = sqlTree.CurrentContainer.Parent?.Parent?.Parent;
                                if (tryBlock == null)
                                    throw new ArgumentNullException(nameof(tryBlock));
                                if (tryBlock.Parent == null)
                                    throw new ArgumentNullException("tryBlockParent");
                                Node tryContainerClose = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_CLOSE, "", tryBlock);
                                ProcessCompoundKeyword(tokenList, sqlTree, tryContainerClose, ref tokenID, significantTokenPositions, 2);
                                sqlTree.CurrentContainer = tryBlock.Parent;
                            }
                            else
                            {
                                ProcessCompoundKeywordWithError(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                            }
                        }
                        else if (significantTokensString.StartsWith("END CATCH "))
                        {
                            sqlTree.EscapeAnySingleOrPartialStatementContainers();

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                                && sqlTree.PathNameMatches(2, SqlStructureConstants.ENAME_CONTAINER_MULTISTATEMENT)
                                && sqlTree.PathNameMatches(3, SqlStructureConstants.ENAME_CATCH_BLOCK)
                                )
                            {
                                //clause.statement.multicontainer.catch
                                Node? catchBlock = sqlTree.CurrentContainer.Parent?.Parent?.Parent;
                                if (catchBlock == null)
                                    throw new ArgumentNullException("catchBlock");
                                if (catchBlock.Parent == null)
                                    throw new ArgumentNullException("catchBlockParent");
                                Node catchContainerClose = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_CLOSE, "", catchBlock);
                                ProcessCompoundKeyword(tokenList, sqlTree, catchContainerClose, ref tokenID, significantTokenPositions, 2);
                                sqlTree.CurrentContainer = catchBlock.Parent;
                            }
                            else
                            {
                                ProcessCompoundKeywordWithError(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                            }
                        }
                        else if (significantTokensString.StartsWith("END "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_CASE_THEN)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(3, SqlStructureConstants.ENAME_CASE_STATEMENT);
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_CLOSE, ""));
                                sqlTree.MoveToAncestorContainer(1); //unnamed container
                            }
                            else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_CASE_ELSE)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlStructureConstants.ENAME_CASE_STATEMENT);
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_CLOSE, ""));
                                sqlTree.MoveToAncestorContainer(1); //unnamed container
                            }
                            else
                            {
                                //Begin/End block handling
                                sqlTree.EscapeAnySingleOrPartialStatementContainers();

                                if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                                    && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                                    && sqlTree.PathNameMatches(2, SqlStructureConstants.ENAME_CONTAINER_MULTISTATEMENT)
                                    && sqlTree.PathNameMatches(3, SqlStructureConstants.ENAME_BEGIN_END_BLOCK)
                                    )
                                {
                                    Node? beginBlock = sqlTree.CurrentContainer.Parent?.Parent?.Parent;
                                    if (beginBlock == null)
                                        throw new ArgumentNullException("beginBlock");
                                    if (beginBlock.Parent == null)
                                        throw new ArgumentNullException("beginBlockParent");
                                    Node beginContainerClose = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_CLOSE, "", beginBlock);
                                    sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, beginContainerClose);
                                    sqlTree.CurrentContainer = beginBlock.Parent;
                                }
                                else
                                {
                                    sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                                }
                            }
                        }
                        else if (significantTokensString.StartsWith("GO "))
                        {
                            sqlTree.EscapeAnySingleOrPartialStatementContainers();

                            if ((tokenID == 0 || IsLineBreakingWhiteSpaceOrComment(tokenList[tokenID - 1]))
                                && IsFollowedByLineBreakingWhiteSpaceOrSingleLineCommentOrEnd(tokenList, tokenID)
                                )
                            {
                                // we found a batch separator - were we supposed to?
                                if (sqlTree.FindValidBatchEnd())
                                {
                                    Node sqlRoot = sqlTree.RootContainer();
                                    Node batchSeparator = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_BATCH_SEPARATOR, "", sqlRoot);
                                    sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, batchSeparator);
                                    sqlTree.StartNewStatement(sqlRoot);
                                }
                                else
                                {
                                    sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                                }
                            }
                            else
                            {
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("EXECUTE AS "))
                        {
                            bool executeAsInWithOptions = false;
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_DDL_WITH_CLAUSE)
                                && (IsLatestTokenAComma(sqlTree)
                                    || !sqlTree.HasNonWhiteSpaceNonCommentContent(sqlTree.CurrentContainer)
                                    )
                                )
                                executeAsInWithOptions = true;

                            if (!executeAsInWithOptions)
                            {
                                sqlTree.ConsiderStartingNewStatement();
                                sqlTree.ConsiderStartingNewClause();
                            }

                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("EXEC ")
                            || significantTokensString.StartsWith("EXECUTE ")
                            )
                        {
                            bool execShouldntTryToStartNewStatement = false;

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                                && (ContentStartsWithKeyword(sqlTree.CurrentContainer, "INSERT")
                                    || ContentStartsWithKeyword(sqlTree.CurrentContainer, "INSERT INTO")
                                    )
                                )
                            {
                                int existingClauseCount = sqlTree.CurrentContainer.Parent != null ? sqlTree.CurrentContainer.Parent.ChildrenByName(SqlStructureConstants.ENAME_SQL_CLAUSE).Count() : 0;
                                if (existingClauseCount == 1)
                                    execShouldntTryToStartNewStatement = true;
                            }

                            if (!execShouldntTryToStartNewStatement)
                                sqlTree.ConsiderStartingNewStatement();

                            sqlTree.ConsiderStartingNewClause();

                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (_JoinDetector.IsMatch(significantTokensString))
                        {
                            sqlTree.ConsiderStartingNewClause();
                            string joinText = _JoinDetector.Match(significantTokensString).Value;
                            int targetKeywordCount = joinText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, targetKeywordCount);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SELECTIONTARGET, "");
                        }
                        else if (significantTokensString.StartsWith("UNION ALL "))
                        {
                            sqlTree.ConsiderStartingNewClause();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SET_OPERATOR_CLAUSE, ""), ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("UNION ")
                            || significantTokensString.StartsWith("INTERSECT ")
                            || significantTokensString.StartsWith("EXCEPT ")
                            )
                        {
                            sqlTree.ConsiderStartingNewClause();
                            Node unionClause = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SET_OPERATOR_CLAUSE, "");
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, unionClause);
                        }
                        else if (significantTokensString.StartsWith("WHILE "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            Node newWhileLoop = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_WHILE_LOOP, "");
                            Node whileContainerOpen = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_OPEN, "", newWhileLoop);
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, whileContainerOpen);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_BOOLEAN_EXPRESSION, "", newWhileLoop);
                        }
                        else if (significantTokensString.StartsWith("IF "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.StartNewContainer(SqlStructureConstants.ENAME_IF_STATEMENT, token.Value, SqlStructureConstants.ENAME_BOOLEAN_EXPRESSION);
                        }
                        else if (significantTokensString.StartsWith("ELSE "))
                        {
                            sqlTree.EscapeAnyBetweenConditions();
                            sqlTree.EscapeAnySelectionTarget();
                            sqlTree.EscapeJoinCondition();

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_CASE_THEN)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(3, SqlStructureConstants.ENAME_CASE_STATEMENT);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_CASE_ELSE, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                sqlTree.EscapePartialStatementContainers();

                                if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                                    && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                                    && sqlTree.PathNameMatches(2, SqlStructureConstants.ENAME_CONTAINER_SINGLESTATEMENT)
                                    )
                                {
                                    //we need to pop up the single-statement containers stack to the next "if" that doesn't have an "else" (if any; else error).
                                    // LOCAL SEARCH - we're not actually changing the "CurrentContainer" until we decide to start a statement.
                                    Node? currentNode = sqlTree.CurrentContainer.Parent?.Parent;
                                    bool stopSearching = false;
                                    while (!stopSearching)
                                    {
                                        if (currentNode == null)
                                            throw new ArgumentNullException("currentNode");

                                        if (sqlTree.PathNameMatches(currentNode, 1, SqlStructureConstants.ENAME_IF_STATEMENT))
                                        {
                                            //if this is in an "If", then the "Else" must still be available - yay!
                                            if (currentNode.Parent == null)
                                                throw new ArgumentNullException("currentNodeParent");
                                            sqlTree.CurrentContainer = currentNode.Parent;
                                            sqlTree.StartNewContainer(SqlStructureConstants.ENAME_ELSE_CLAUSE, token.Value, SqlStructureConstants.ENAME_CONTAINER_SINGLESTATEMENT);
                                            sqlTree.StartNewStatement();
                                            stopSearching = true;
                                        }
                                        else if (sqlTree.PathNameMatches(currentNode, 1, SqlStructureConstants.ENAME_ELSE_CLAUSE))
                                        {
                                            //If this is in an "Else", we should skip its parent "IF" altogether, and go to the next singlestatementcontainer candidate.
                                            //singlestatementcontainer.else.if.clause.statement.NEWCANDIDATE
                                            currentNode = currentNode.Parent?.Parent?.Parent?.Parent?.Parent;
                                        }
                                        else if (sqlTree.PathNameMatches(currentNode, 1, SqlStructureConstants.ENAME_WHILE_LOOP))
                                        {
                                            //If this is in a "While", we should skip to the next singlestatementcontainer candidate.
                                            //singlestatementcontainer.while.clause.statement.NEWCANDIDATE
                                            currentNode = currentNode.Parent?.Parent?.Parent?.Parent;
                                        }
                                        else
                                        {
                                            //if this isn't a known single-statement container, then we're lost.
                                            sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                                            stopSearching = true;
                                        }
                                    }
                                }
                                else
                                {
                                    sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                                }
                            }
                        }
                        else if (significantTokensString.StartsWith("INSERT INTO "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.ConsiderStartingNewClause();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL CHARACTER VARYING "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 3);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL CHAR VARYING "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 3);
                        }
                        else if (significantTokensString.StartsWith("BINARY VARYING "))
                        {
                            //TODO: Figure out how to handle "Compound Keyword Datatypes" so they are still correctly highlighted
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("CHAR VARYING "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("CHARACTER VARYING "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("DOUBLE PRECISION "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL CHARACTER "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL CHAR "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("NATIONAL TEXT "))
                        {
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("INSERT "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.ConsiderStartingNewClause();
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("BULK INSERT "))
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.ConsiderStartingNewClause();
                            ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
                        }
                        else if (significantTokensString.StartsWith("SELECT "))
                        {
                            if (sqlTree.NewStatementDue)
                                sqlTree.ConsiderStartingNewStatement();

                            bool selectShouldntTryToStartNewStatement = false;

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE))
                            {
                                Node? firstStatementClause = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer.Parent);

                                bool isPrecededByInsertStatement = false;
                                if (sqlTree.CurrentContainer.Parent != null)
                                    foreach (Node clause in sqlTree.CurrentContainer.Parent.ChildrenByName(SqlStructureConstants.ENAME_SQL_CLAUSE))
                                        if (ContentStartsWithKeyword(clause, "INSERT"))
                                            isPrecededByInsertStatement = true;

                                if (isPrecededByInsertStatement)
                                {
                                    bool existingSelectClauseFound = false;
                                    if (sqlTree.CurrentContainer.Parent != null)
                                        foreach (Node clause in sqlTree.CurrentContainer.Parent.ChildrenByName(SqlStructureConstants.ENAME_SQL_CLAUSE))
                                            if (ContentStartsWithKeyword(clause, "SELECT"))
                                                existingSelectClauseFound = true;

                                    bool existingValuesClauseFound = false;
                                    if (sqlTree.CurrentContainer.Parent != null)
                                        foreach (Node clause in sqlTree.CurrentContainer.Parent.ChildrenByName(SqlStructureConstants.ENAME_SQL_CLAUSE))
                                            if (ContentStartsWithKeyword(clause, "VALUES"))
                                                existingValuesClauseFound = true;

                                    bool existingExecClauseFound = false;
                                    if (sqlTree.CurrentContainer.Parent != null)
                                        foreach (Node clause in sqlTree.CurrentContainer.Parent.ChildrenByName(SqlStructureConstants.ENAME_SQL_CLAUSE))
                                            if (ContentStartsWithKeyword(clause, "EXEC")
                                                || ContentStartsWithKeyword(clause, "EXECUTE"))
                                                existingExecClauseFound = true;

                                    if (!existingSelectClauseFound
                                        && !existingValuesClauseFound
                                        && !existingExecClauseFound
                                        )
                                        selectShouldntTryToStartNewStatement = true;
                                }

                                Node? firstEntryOfThisClause = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
                                if (firstEntryOfThisClause != null && firstEntryOfThisClause.Name.Equals(SqlStructureConstants.ENAME_SET_OPERATOR_CLAUSE))
                                    selectShouldntTryToStartNewStatement = true;
                            }

                            if (!selectShouldntTryToStartNewStatement)
                                sqlTree.ConsiderStartingNewStatement();

                            sqlTree.ConsiderStartingNewClause();

                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("UPDATE "))
                        {
                            if (sqlTree.NewStatementDue)
                                sqlTree.ConsiderStartingNewStatement();

                            if (!(sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                    && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_CURSOR_FOR_OPTIONS)
                                    )
                                )
                            {
                                sqlTree.ConsiderStartingNewStatement();
                                sqlTree.ConsiderStartingNewClause();
                            }

                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("TO "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_PERMISSIONS_TARGET)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlStructureConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_PERMISSIONS_RECIPIENT, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                //I don't currently know whether there is any other place where "TO" can be used in T-SQL...
                                // TODO: look into that.
                                // -> for now, we'll just save as a random keyword without raising an error.
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (significantTokensString.StartsWith("FROM "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_PERMISSIONS_TARGET)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlStructureConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_PERMISSIONS_RECIPIENT, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else
                            {
                                sqlTree.ConsiderStartingNewClause();
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                                sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SELECTIONTARGET, "");
                            }
                        }
                        else if (significantTokensString.StartsWith("CASCADE ")
                            && sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                            && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_PERMISSIONS_RECIPIENT)
                            )
                        {
                            sqlTree.MoveToAncestorContainer(2, SqlStructureConstants.ENAME_PERMISSIONS_BLOCK);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT, "", sqlTree.SaveNewElement(SqlStructureConstants.ENAME_DDL_WITH_CLAUSE, ""));
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("SET "))
                        {
                            Node? firstNonCommentSibling2 = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
                            if (!(
                                    firstNonCommentSibling2 != null
                                    && firstNonCommentSibling2.Name.Equals(SqlStructureConstants.ENAME_OTHERKEYWORD)
                                    && (firstNonCommentSibling2.TextValue ?? "").ToUpperInvariant().StartsWith("UPDATE")
                                    )
                                )
                                sqlTree.ConsiderStartingNewStatement();

                            sqlTree.ConsiderStartingNewClause();
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                        }
                        else if (significantTokensString.StartsWith("BETWEEN "))
                        {
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_BETWEEN_CONDITION, "");
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_OPEN, ""));
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_BETWEEN_LOWERBOUND, "");
                        }
                        else if (significantTokensString.StartsWith("AND "))
                        {
                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_BETWEEN_LOWERBOUND))
                            {
                                sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_BETWEEN_CONDITION);
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_CLOSE, ""));
                                sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_BETWEEN_UPPERBOUND, "");
                            }
                            else
                            {
                                sqlTree.EscapeAnyBetweenConditions();
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_AND_OPERATOR, ""));
                            }
                        }
                        else if (significantTokensString.StartsWith("OR "))
                        {
                            sqlTree.EscapeAnyBetweenConditions();
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OR_OPERATOR, ""));
                        }
                        else if (significantTokensString.StartsWith("WITH "))
                        {
                            if (sqlTree.NewStatementDue)
                                sqlTree.ConsiderStartingNewStatement();

                            if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                                && !sqlTree.HasNonWhiteSpaceNonCommentContent(sqlTree.CurrentContainer)
                                )
                            {
                                sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CTE_WITH_CLAUSE, "");
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value, sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_OPEN, ""));
                                sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CTE_ALIAS, "");
                            }
                            else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                                && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_PERMISSIONS_RECIPIENT)
                                )
                            {
                                sqlTree.MoveToAncestorContainer(2, SqlStructureConstants.ENAME_PERMISSIONS_BLOCK);
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_DDL_WITH_CLAUSE, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_DDL_PROCEDURAL_BLOCK)
                                || sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_DDL_OTHER_BLOCK)
                                )
                            {
                                sqlTree.StartNewContainer(SqlStructureConstants.ENAME_DDL_WITH_CLAUSE, token.Value, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT);
                            }
                            else if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SELECTIONTARGET))
                            {
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                            else
                            {
                                sqlTree.ConsiderStartingNewClause();
                                sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERKEYWORD, token.Value);
                            }
                        }
                        else if (tokenList.Count > tokenID + 1
                            && tokenList[tokenID + 1].Type == SqlTokenType.Colon
                            && !(tokenList.Count > tokenID + 2
                                && tokenList[tokenID + 2].Type == SqlTokenType.Colon
                                )
                            )
                        {
                            sqlTree.ConsiderStartingNewStatement();
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_LABEL, token.Value + tokenList[tokenID + 1].Value);
                            tokenID++;
                        }
                        else
                        {
                            //miscellaneous single-word tokens, which may or may not be statement starters and/or clause starters

                            //check for statements starting...
                            if (IsStatementStarter(token) || sqlTree.NewStatementDue)
                            {
                                sqlTree.ConsiderStartingNewStatement();
                            }

                            //check for statements starting...
                            if (IsClauseStarter(token))
                            {
                                sqlTree.ConsiderStartingNewClause();
                            }

                            string newNodeName = SqlStructureConstants.ENAME_OTHERNODE;
                            KeywordType matchedKeywordType;
                            if (KeywordList.TryGetValue(token.Value.ToUpperInvariant(), out matchedKeywordType))
                            {
                                switch (matchedKeywordType)
                                {
                                    case KeywordType.OperatorKeyword:
                                        newNodeName = SqlStructureConstants.ENAME_ALPHAOPERATOR;
                                        break;
                                    case KeywordType.FunctionKeyword:
                                        newNodeName = SqlStructureConstants.ENAME_FUNCTION_KEYWORD;
                                        break;
                                    case KeywordType.DataTypeKeyword:
                                        newNodeName = SqlStructureConstants.ENAME_DATATYPE_KEYWORD;
                                        break;
                                    case KeywordType.OtherKeyword:
                                        sqlTree.EscapeAnySelectionTarget();
                                        newNodeName = SqlStructureConstants.ENAME_OTHERKEYWORD;
                                        break;
                                    default:
                                        throw new Exception("Unrecognized Keyword Type!");
                                }
                            }

                            sqlTree.SaveNewElement(newNodeName, token.Value);
                        }
                        break;

                    case SqlTokenType.Semicolon:
                        sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SEMICOLON, token.Value);
                        sqlTree.NewStatementDue = true;
                        break;

                    case SqlTokenType.Colon:
                        if (tokenList.Count > tokenID + 1
                            && tokenList[tokenID + 1].Type == SqlTokenType.Colon
                            )
                        {
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_SCOPERESOLUTIONOPERATOR, token.Value + tokenList[tokenID + 1].Value);
                            tokenID++;
                        }
                        else if (tokenList.Count > tokenID + 1
                            && tokenList[tokenID + 1].Type == SqlTokenType.OtherNode
                            )
                        {
                            //This SHOULD never happen in valid T-SQL, but can happen in DB2 or NexusDB or PostgreSQL 
                            // code (host variables) - so be nice and handle it anyway.
                            sqlTree.SaveNewElement(SqlStructureConstants.ENAME_OTHERNODE, token.Value + tokenList[tokenID + 1].Value);
                            tokenID++;
                        }
                        else
                        {
                            sqlTree.SaveNewElementWithError(SqlStructureConstants.ENAME_OTHEROPERATOR, token.Value);
                        }
                        break;

                    case SqlTokenType.Comma:
                        bool isCTESplitter = (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT)
                            && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_CTE_WITH_CLAUSE)
                            );

                        sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);

                        if (isCTESplitter)
                        {
                            sqlTree.MoveToAncestorContainer(1, SqlStructureConstants.ENAME_CTE_WITH_CLAUSE);
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CTE_ALIAS, "");
                        }
                        break;

                    case SqlTokenType.EqualsSign:
                        sqlTree.SaveNewElement(SqlStructureConstants.ENAME_EQUALSSIGN, token.Value);
                        if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_DDL_DECLARE_BLOCK))
                            sqlTree.CurrentContainer = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_CONTAINER_GENERALCONTENT, "");
                        break;

                    case SqlTokenType.MultiLineComment:
                    case SqlTokenType.SingleLineComment:
                    case SqlTokenType.SingleLineCommentCStyle:
                    case SqlTokenType.WhiteSpace:
                        //create in statement rather than clause if there are no siblings yet
                        if (sqlTree.PathNameMatches(0, SqlStructureConstants.ENAME_SQL_CLAUSE)
                            && sqlTree.PathNameMatches(1, SqlStructureConstants.ENAME_SQL_STATEMENT)
                            && !sqlTree.CurrentContainer.Children.Any()
                            )
                            sqlTree.SaveNewElementAsPriorSibling(GetEquivalentSqlNodeName(token.Type), token.Value, sqlTree.CurrentContainer);
                        else
                            sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);
                        break;

                    case SqlTokenType.BracketQuotedName:
                    case SqlTokenType.Asterisk:
                    case SqlTokenType.Period:
                    case SqlTokenType.OtherOperator:
                    case SqlTokenType.NationalString:
                    case SqlTokenType.String:
                    case SqlTokenType.QuotedString:
                    case SqlTokenType.Number:
                    case SqlTokenType.BinaryValue:
                    case SqlTokenType.MonetaryValue:
                    case SqlTokenType.PseudoName:
                        sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);
                        break;
                    default:
                        throw new Exception("Unrecognized element encountered!");
                }

                tokenID++;
            }

            if (tokenList.HasUnfinishedToken)
                sqlTree.SetError();

            if (!sqlTree.FindValidBatchEnd())
                sqlTree.SetError();

            return sqlTree;
        }

        //TODO: move into parse tree
        private static bool ContentStartsWithKeyword(Node providedContainer, string? contentToMatch)
        {
            Node? firstEntryOfProvidedContainer = ((ParseTree)providedContainer.RootContainer()).GetFirstNonWhitespaceNonCommentChildElement(providedContainer);
            bool targetFound;
            string? keywordUpperValue = null;

            if (firstEntryOfProvidedContainer != null
                && firstEntryOfProvidedContainer.Name.Equals(SqlStructureConstants.ENAME_OTHERKEYWORD)
                && firstEntryOfProvidedContainer.TextValue != null
                )
                keywordUpperValue = firstEntryOfProvidedContainer.TextValue.ToUpperInvariant();

            if (firstEntryOfProvidedContainer != null
                && firstEntryOfProvidedContainer.Name.Equals(SqlStructureConstants.ENAME_COMPOUNDKEYWORD)
                )
                keywordUpperValue = firstEntryOfProvidedContainer.GetAttributeValue(SqlStructureConstants.ANAME_SIMPLETEXT);

            if (keywordUpperValue != null)
            {
                targetFound = keywordUpperValue.Equals(contentToMatch) || keywordUpperValue.StartsWith(contentToMatch + " ");
            }
            else
            {
                //if contentToMatch was passed in as null, means we were looking for a NON-keyword.
                targetFound = contentToMatch == null;
            }

            return targetFound;
        }

        private void ProcessCompoundKeywordWithError(ITokenList tokenList, ParseTree sqlTree, Node currentContainerElement, ref int tokenID, List<int> significantTokenPositions, int keywordCount)
        {
            ProcessCompoundKeyword(tokenList, sqlTree, currentContainerElement, ref tokenID, significantTokenPositions, keywordCount);
            sqlTree.SetError();
        }

        private void ProcessCompoundKeyword(ITokenList tokenList, ParseTree sqlTree, Node targetContainer, ref int tokenID, List<int> significantTokenPositions, int keywordCount)
        {
            Node compoundKeyword = sqlTree.SaveNewElement(SqlStructureConstants.ENAME_COMPOUNDKEYWORD, "", targetContainer);
            string targetText = ExtractTokensString(tokenList, significantTokenPositions.GetRange(0, keywordCount)).TrimEnd();
            compoundKeyword.SetAttribute(SqlStructureConstants.ANAME_SIMPLETEXT, targetText);
            AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[0], significantTokenPositions[keywordCount - 1]), SqlStructureConstants.ENAME_OTHERKEYWORD, compoundKeyword);
            tokenID = significantTokenPositions[keywordCount - 1];
        }

        private void AppendNodesWithMapping(ParseTree sqlTree, IEnumerable<IToken> tokens, string otherTokenMappingName, Node targetContainer)
        {
            foreach (var token in tokens)
            {
                string elementName;
                if (token.Type == SqlTokenType.OtherNode)
                    elementName = otherTokenMappingName;
                else
                    elementName = GetEquivalentSqlNodeName(token.Type);

                sqlTree.SaveNewElement(elementName, token.Value, targetContainer);
            }
        }

        private string ExtractTokensString(ITokenList tokenList, IList<int> significantTokenPositions)
        {
            StringBuilder keywordSB = new StringBuilder();
            foreach (int tokenPos in significantTokenPositions)
            {
                //grr, this could be more elegant.
                if (tokenList[tokenPos].Type == SqlTokenType.Comma)
                    keywordSB.Append(",");
                else
                    keywordSB.Append(tokenList[tokenPos].Value.ToUpperInvariant());
                keywordSB.Append(" ");
            }
            return keywordSB.ToString();
        }

        private string GetEquivalentSqlNodeName(SqlTokenType tokenType)
        {
            switch (tokenType)
            {
                case SqlTokenType.WhiteSpace:
                    return SqlStructureConstants.ENAME_WHITESPACE;
                case SqlTokenType.SingleLineComment:
                    return SqlStructureConstants.ENAME_COMMENT_SINGLELINE;
                case SqlTokenType.SingleLineCommentCStyle:
                    return SqlStructureConstants.ENAME_COMMENT_SINGLELINE_CSTYLE;
                case SqlTokenType.MultiLineComment:
                    return SqlStructureConstants.ENAME_COMMENT_MULTILINE;
                case SqlTokenType.BracketQuotedName:
                    return SqlStructureConstants.ENAME_BRACKET_QUOTED_NAME;
                case SqlTokenType.Asterisk:
                    return SqlStructureConstants.ENAME_ASTERISK;
                case SqlTokenType.EqualsSign:
                    return SqlStructureConstants.ENAME_EQUALSSIGN;
                case SqlTokenType.Comma:
                    return SqlStructureConstants.ENAME_COMMA;
                case SqlTokenType.Period:
                    return SqlStructureConstants.ENAME_PERIOD;
                case SqlTokenType.NationalString:
                    return SqlStructureConstants.ENAME_NSTRING;
                case SqlTokenType.String:
                    return SqlStructureConstants.ENAME_STRING;
                case SqlTokenType.QuotedString:
                    return SqlStructureConstants.ENAME_QUOTED_STRING;
                case SqlTokenType.OtherOperator:
                    return SqlStructureConstants.ENAME_OTHEROPERATOR;
                case SqlTokenType.Number:
                    return SqlStructureConstants.ENAME_NUMBER_VALUE;
                case SqlTokenType.MonetaryValue:
                    return SqlStructureConstants.ENAME_MONETARY_VALUE;
                case SqlTokenType.BinaryValue:
                    return SqlStructureConstants.ENAME_BINARY_VALUE;
                case SqlTokenType.PseudoName:
                    return SqlStructureConstants.ENAME_PSEUDONAME;
                default:
                    throw new Exception("Mapping not found for provided Token Type");
            }
        }

        private string GetKeywordMatchPhrase(ITokenList tokenList, int tokenID, ref List<string> rawKeywordParts, ref List<int> tokenCounts, ref List<List<IToken>> overflowNodes)
        {
            string phrase = "";
            int phraseComponentsFound = 0;
            rawKeywordParts = new List<string>();
            overflowNodes = new List<List<IToken>>();
            tokenCounts = new List<int>();
            string precedingWhitespace = "";
            int originalTokenID = tokenID;

            while (tokenID < tokenList.Count && phraseComponentsFound < 7)
            {
                if (tokenList[tokenID].Type == SqlTokenType.OtherNode
                    || tokenList[tokenID].Type == SqlTokenType.BracketQuotedName
                    || tokenList[tokenID].Type == SqlTokenType.Comma
                    )
                {
                    phrase += tokenList[tokenID].Value.ToUpperInvariant() + " ";
                    phraseComponentsFound++;
                    rawKeywordParts.Add(precedingWhitespace + tokenList[tokenID].Value);

                    tokenID++;
                    tokenCounts.Add(tokenID - originalTokenID);

                    //found a possible phrase component - skip past any upcoming whitespace or comments, keeping track.
                    overflowNodes.Add(new List<IToken>());
                    precedingWhitespace = "";
                    while (tokenID < tokenList.Count
                        && (tokenList[tokenID].Type == SqlTokenType.WhiteSpace
                            || tokenList[tokenID].Type == SqlTokenType.SingleLineComment
                            || tokenList[tokenID].Type == SqlTokenType.MultiLineComment
                            )
                        )
                    {
                        if (tokenList[tokenID].Type == SqlTokenType.WhiteSpace)
                            precedingWhitespace += tokenList[tokenID].Value;
                        else
                            overflowNodes[phraseComponentsFound-1].Add(tokenList[tokenID]);

                        tokenID++;
                    }
                }
                else
                    //we're not interested in any other node types
                    break;
            }

            return phrase;
        }

        private List<int> GetSignificantTokenPositions(ITokenList tokenList, int tokenID, int searchDistance)
        {
            List<int> significantTokenPositions = new List<int>();
            int originalTokenID = tokenID;

            while (tokenID < tokenList.Count && significantTokenPositions.Count < searchDistance)
            {
                if (tokenList[tokenID].Type == SqlTokenType.OtherNode
                    || tokenList[tokenID].Type == SqlTokenType.BracketQuotedName
                    || tokenList[tokenID].Type == SqlTokenType.Comma
                    )
                {
                    significantTokenPositions.Add(tokenID);
                    tokenID++;

                    //found a possible phrase component - skip past any upcoming whitespace or comments, keeping track.
                    while (tokenID < tokenList.Count
                        && (tokenList[tokenID].Type == SqlTokenType.WhiteSpace
                            || tokenList[tokenID].Type == SqlTokenType.SingleLineComment
                            || tokenList[tokenID].Type == SqlTokenType.MultiLineComment
                            )
                        )
                    {
                        tokenID++;
                    }
                }
                else
                    //we're not interested in any other node types
                    break;
            }

            return significantTokenPositions;
        }

        private Node ProcessCompoundKeyword(ParseTree sqlTree, string newElementName, ref int tokenID, Node currentContainerElement, int compoundKeywordCount, List<int> compoundKeywordTokenCounts, List<string> compoundKeywordRawStrings)
        {
            Node newElement = NodeFactory.CreateNode(newElementName, GetCompoundKeyword(ref tokenID, compoundKeywordCount, compoundKeywordTokenCounts, compoundKeywordRawStrings));
            sqlTree.CurrentContainer.AddChild(newElement);
            return newElement;
        }

        private string GetCompoundKeyword(ref int tokenID, int compoundKeywordCount, List<int> compoundKeywordTokenCounts, List<string> compoundKeywordRawStrings)
        {
            tokenID += compoundKeywordTokenCounts[compoundKeywordCount - 1] - 1;
            string outString = "";
            for (int i = 0; i < compoundKeywordCount; i++)
                outString += compoundKeywordRawStrings[i];
            return outString;
        }

        private static bool IsStatementStarter(IToken token)
        {
            //List created from experience, and augmented with individual sections of MSDN:
            // http://msdn.microsoft.com/en-us/library/ff848799.aspx
            // http://msdn.microsoft.com/en-us/library/ff848727.aspx
            // http://msdn.microsoft.com/en-us/library/ms174290.aspx
            // etc...
            string uppercaseValue = token.Value.ToUpperInvariant();
            return (token.Type == SqlTokenType.OtherNode
                && (uppercaseValue.Equals("ALTER")
                    || uppercaseValue.Equals("BACKUP")
                    || uppercaseValue.Equals("BREAK")
                    || uppercaseValue.Equals("CLOSE")
                    || uppercaseValue.Equals("CHECKPOINT")
                    || uppercaseValue.Equals("COMMIT")
                    || uppercaseValue.Equals("CONTINUE")
                    || uppercaseValue.Equals("CREATE")
                    || uppercaseValue.Equals("DBCC")
                    || uppercaseValue.Equals("DEALLOCATE")
                    || uppercaseValue.Equals("DELETE")
                    || uppercaseValue.Equals("DECLARE")
                    || uppercaseValue.Equals("DENY")
                    || uppercaseValue.Equals("DROP")
                    || uppercaseValue.Equals("EXEC")
                    || uppercaseValue.Equals("EXECUTE")
                    || uppercaseValue.Equals("FETCH")
                    || uppercaseValue.Equals("GOTO")
                    || uppercaseValue.Equals("GRANT")
                    || uppercaseValue.Equals("IF")
                    || uppercaseValue.Equals("INSERT")
                    || uppercaseValue.Equals("KILL")
                    || uppercaseValue.Equals("MERGE")
                    || uppercaseValue.Equals("OPEN")
                    || uppercaseValue.Equals("PRINT")
                    || uppercaseValue.Equals("RAISERROR")
                    || uppercaseValue.Equals("RECONFIGURE")
                    || uppercaseValue.Equals("RESTORE")
                    || uppercaseValue.Equals("RETURN")
                    || uppercaseValue.Equals("REVERT")
                    || uppercaseValue.Equals("REVOKE")
                    || uppercaseValue.Equals("SELECT")
                    || uppercaseValue.Equals("SET")
                    || uppercaseValue.Equals("SETUSER")
                    || uppercaseValue.Equals("SHUTDOWN")
                    || uppercaseValue.Equals("TRUNCATE")
                    || uppercaseValue.Equals("UPDATE")
                    || uppercaseValue.Equals("USE")
                    || uppercaseValue.Equals("WAITFOR")
                    || uppercaseValue.Equals("WHILE")
                    )
                );
        }

        private static bool IsClauseStarter(IToken token)
        {
            //Note: some clause starters are handled separately: Joins, RETURNS clauses, etc.
            string uppercaseValue = token.Value.ToUpperInvariant();
            return (token.Type == SqlTokenType.OtherNode
                && (uppercaseValue.Equals("DELETE")
                    || uppercaseValue.Equals("EXCEPT")
                    || uppercaseValue.Equals("FOR")
                    || uppercaseValue.Equals("FROM")
                    || uppercaseValue.Equals("GROUP")
                    || uppercaseValue.Equals("HAVING")
                    || uppercaseValue.Equals("INNER")
                    || uppercaseValue.Equals("INTERSECT")
                    || uppercaseValue.Equals("INTO")
                    || uppercaseValue.Equals("INSERT")
                    || uppercaseValue.Equals("MERGE")
                    || uppercaseValue.Equals("ORDER")
                    || uppercaseValue.Equals("OUTPUT") //this is complicated... in sprocs output means something else!
                    || uppercaseValue.Equals("PIVOT")
                    || uppercaseValue.Equals("RETURNS")
                    || uppercaseValue.Equals("SELECT")
                    || uppercaseValue.Equals("UNION")
                    || uppercaseValue.Equals("UNPIVOT")
                    || uppercaseValue.Equals("UPDATE")
                    || uppercaseValue.Equals("USING")
                    || uppercaseValue.Equals("VALUES")
                    || uppercaseValue.Equals("WHERE")
                    || uppercaseValue.Equals("WITH")
                    )
                );
        }

        private bool IsLatestTokenADDLDetailValue(ParseTree sqlTree)
        {
            var latestContentNode = sqlTree.CurrentContainer.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).LastOrDefault();
            if (latestContentNode != null
                && (latestContentNode.Name.Equals(SqlStructureConstants.ENAME_OTHERKEYWORD)
                    || latestContentNode.Name.Equals(SqlStructureConstants.ENAME_DATATYPE_KEYWORD)
                    || latestContentNode.Name.Equals(SqlStructureConstants.ENAME_COMPOUNDKEYWORD)
                    ))
            {
                string uppercaseText;
                if (latestContentNode.Name.Equals(SqlStructureConstants.ENAME_COMPOUNDKEYWORD))
                    uppercaseText = latestContentNode.GetAttributeValue(SqlStructureConstants.ANAME_SIMPLETEXT) ?? "";
                else
                    uppercaseText = (latestContentNode.TextValue ?? "").ToUpperInvariant();

                return (
                    uppercaseText.Equals("NVARCHAR")
                    || uppercaseText.Equals("VARCHAR")
                    || uppercaseText.Equals("DECIMAL")
                    || uppercaseText.Equals("DEC")
                    || uppercaseText.Equals("NUMERIC")
                    || uppercaseText.Equals("VARBINARY")
                    || uppercaseText.Equals("DEFAULT")
                    || uppercaseText.Equals("IDENTITY")
                    || uppercaseText.Equals("XML")
                    || uppercaseText.EndsWith("VARYING")
                    || uppercaseText.EndsWith("CHAR")
                    || uppercaseText.EndsWith("CHARACTER")
                    || uppercaseText.Equals("FLOAT")
                    || uppercaseText.Equals("DATETIMEOFFSET")
                    || uppercaseText.Equals("DATETIME2")
                    || uppercaseText.Equals("TIME")
                    );
            }
            return false;
        }

        private bool IsLatestTokenAComma(ParseTree sqlTree)
        {
            var latestContent = sqlTree.CurrentContainer.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).LastOrDefault();
            return latestContent != null && latestContent.Name.Equals(SqlStructureConstants.ENAME_COMMA);
        }

        private bool IsLatestTokenAMiscName(ParseTree sqlTree)
        {
            var latestContent = sqlTree.CurrentContainer.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).LastOrDefault();

            if (latestContent != null)
            {
                string testValue = (latestContent.TextValue ?? "").ToUpperInvariant();

                if (latestContent.Name.Equals(SqlStructureConstants.ENAME_BRACKET_QUOTED_NAME))
                    return true;

                if ((latestContent.Name.Equals(SqlStructureConstants.ENAME_OTHERNODE)
                        || latestContent.Name.Equals(SqlStructureConstants.ENAME_FUNCTION_KEYWORD)
                        )
                        && !(testValue.Equals("AND")
                            || testValue.Equals("OR")
                            || testValue.Equals("NOT")
                            || testValue.Equals("BETWEEN")
                            || testValue.Equals("LIKE")
                            || testValue.Equals("CONTAINS")
                            || testValue.Equals("EXISTS")
                            || testValue.Equals("FREETEXT")
                            || testValue.Equals("IN")
                            || testValue.Equals("ALL")
                            || testValue.Equals("SOME")
                            || testValue.Equals("ANY")
                            || testValue.Equals("FROM")
                            || testValue.Equals("JOIN")
                            || testValue.EndsWith(" JOIN")
                            || testValue.Equals("UNION")
                            || testValue.Equals("UNION ALL")
                            || testValue.Equals("USING")
                            || testValue.Equals("AS")
                            || testValue.EndsWith(" APPLY")
                            )
                        )
                    return true;
            }

            return false;
        }

        private static bool IsLineBreakingWhiteSpaceOrComment(IToken token)
        {
            return (token.Type == SqlTokenType.WhiteSpace
                    && Regex.IsMatch(token.Value, @"(\r|\n)+"))
                || token.Type == SqlTokenType.SingleLineComment;
        }

        private bool IsFollowedByLineBreakingWhiteSpaceOrSingleLineCommentOrEnd(ITokenList tokenList, int tokenID)
        {
            int currTokenID = tokenID + 1;
            while (tokenList.Count >= currTokenID + 1)
            {
                if (tokenList[currTokenID].Type == SqlTokenType.SingleLineComment)
                    return true;
                else if (tokenList[currTokenID].Type == SqlTokenType.WhiteSpace)
                {
                    if (Regex.IsMatch(tokenList[currTokenID].Value, @"(\r|\n)+"))
                        return true;
                    else
                        currTokenID++;
                }
                else
                    return false;
            }
            return true;
        }

        private static void InitializeKeywordList()
        {
            //List originally copied from Side by Side SQL Comparer project from CodeProject:
            // http://www.codeproject.com/KB/database/SideBySideSQLComparer.aspx
            // Added some entries that are not strictly speaking keywords, such as 
            // cursor options "READ_ONLY", "FAST_FORWARD", etc.
            // also added numerous missing entries, such as "Xml", etc
            // Could/Should check against MSDN Ref: http://msdn.microsoft.com/en-us/library/ms189822.aspx
            KeywordList = new Dictionary<string, KeywordType>
            {
                { "@@CONNECTIONS", KeywordType.FunctionKeyword },
                { "@@CPU_BUSY", KeywordType.FunctionKeyword },
                { "@@CURSOR_ROWS", KeywordType.FunctionKeyword },
                { "@@DATEFIRST", KeywordType.FunctionKeyword },
                { "@@DBTS", KeywordType.FunctionKeyword },
                { "@@ERROR", KeywordType.FunctionKeyword },
                { "@@FETCH_STATUS", KeywordType.FunctionKeyword },
                { "@@IDENTITY", KeywordType.FunctionKeyword },
                { "@@IDLE", KeywordType.FunctionKeyword },
                { "@@IO_BUSY", KeywordType.FunctionKeyword },
                { "@@LANGID", KeywordType.FunctionKeyword },
                { "@@LANGUAGE", KeywordType.FunctionKeyword },
                { "@@LOCK_TIMEOUT", KeywordType.FunctionKeyword },
                { "@@MAX_CONNECTIONS", KeywordType.FunctionKeyword },
                { "@@MAX_PRECISION", KeywordType.FunctionKeyword },
                { "@@NESTLEVEL", KeywordType.FunctionKeyword },
                { "@@OPTIONS", KeywordType.FunctionKeyword },
                { "@@PACKET_ERRORS", KeywordType.FunctionKeyword },
                { "@@PACK_RECEIVED", KeywordType.FunctionKeyword },
                { "@@PACK_SENT", KeywordType.FunctionKeyword },
                { "@@PROCID", KeywordType.FunctionKeyword },
                { "@@REMSERVER", KeywordType.FunctionKeyword },
                { "@@ROWCOUNT", KeywordType.FunctionKeyword },
                { "@@SERVERNAME", KeywordType.FunctionKeyword },
                { "@@SERVICENAME", KeywordType.FunctionKeyword },
                { "@@SPID", KeywordType.FunctionKeyword },
                { "@@TEXTSIZE", KeywordType.FunctionKeyword },
                { "@@TIMETICKS", KeywordType.FunctionKeyword },
                { "@@TOTAL_ERRORS", KeywordType.FunctionKeyword },
                { "@@TOTAL_READ", KeywordType.FunctionKeyword },
                { "@@TOTAL_WRITE", KeywordType.FunctionKeyword },
                { "@@TRANCOUNT", KeywordType.FunctionKeyword },
                { "@@VERSION", KeywordType.FunctionKeyword },
                { "ABS", KeywordType.FunctionKeyword },
                { "ACOS", KeywordType.FunctionKeyword },
                { "ACTIVATION", KeywordType.OtherKeyword },
                { "ADD", KeywordType.OtherKeyword },
                { "ALL", KeywordType.OperatorKeyword },
                { "ALTER", KeywordType.OtherKeyword },
                { "AND", KeywordType.OperatorKeyword },
                { "ANSI_DEFAULTS", KeywordType.OtherKeyword },
                { "ANSI_NULLS", KeywordType.OtherKeyword },
                { "ANSI_NULL_DFLT_OFF", KeywordType.OtherKeyword },
                { "ANSI_NULL_DFLT_ON", KeywordType.OtherKeyword },
                { "ANSI_PADDING", KeywordType.OtherKeyword },
                { "ANSI_WARNINGS", KeywordType.OtherKeyword },
                { "ANY", KeywordType.OperatorKeyword },
                { "APP_NAME", KeywordType.FunctionKeyword },
                { "ARITHABORT", KeywordType.OtherKeyword },
                { "ARITHIGNORE", KeywordType.OtherKeyword },
                { "AS", KeywordType.OtherKeyword },
                { "ASC", KeywordType.OtherKeyword },
                { "ASCII", KeywordType.FunctionKeyword },
                { "ASIN", KeywordType.FunctionKeyword },
                { "ATAN", KeywordType.FunctionKeyword },
                { "ATN2", KeywordType.FunctionKeyword },
                { "AUTHORIZATION", KeywordType.OtherKeyword },
                { "AVG", KeywordType.FunctionKeyword },
                { "BACKUP", KeywordType.OtherKeyword },
                { "BEGIN", KeywordType.OtherKeyword },
                { "BETWEEN", KeywordType.OperatorKeyword },
                { "BIGINT", KeywordType.DataTypeKeyword },
                { "BINARY", KeywordType.DataTypeKeyword },
                { "BIT", KeywordType.DataTypeKeyword },
                { "BREAK", KeywordType.OtherKeyword },
                { "BROWSE", KeywordType.OtherKeyword },
                { "BULK", KeywordType.OtherKeyword },
                { "BY", KeywordType.OtherKeyword },
                { "CALLER", KeywordType.OtherKeyword },
                { "CASCADE", KeywordType.OtherKeyword },
                { "CASE", KeywordType.FunctionKeyword },
                { "CAST", KeywordType.FunctionKeyword },
                { "CATALOG", KeywordType.OtherKeyword },
                { "CEILING", KeywordType.FunctionKeyword },
                { "CHAR", KeywordType.DataTypeKeyword },
                { "CHARACTER", KeywordType.DataTypeKeyword },
                { "CHARINDEX", KeywordType.FunctionKeyword },
                { "CHECK", KeywordType.OtherKeyword },
                { "CHECKALLOC", KeywordType.OtherKeyword },
                { "CHECKCATALOG", KeywordType.OtherKeyword },
                { "CHECKCONSTRAINTS", KeywordType.OtherKeyword },
                { "CHECKDB", KeywordType.OtherKeyword },
                { "CHECKFILEGROUP", KeywordType.OtherKeyword },
                { "CHECKIDENT", KeywordType.OtherKeyword },
                { "CHECKPOINT", KeywordType.OtherKeyword },
                { "CHECKSUM", KeywordType.FunctionKeyword },
                { "CHECKSUM_AGG", KeywordType.FunctionKeyword },
                { "CHECKTABLE", KeywordType.OtherKeyword },
                { "CLEANTABLE", KeywordType.OtherKeyword },
                { "CLOSE", KeywordType.OtherKeyword },
                { "CLUSTERED", KeywordType.OtherKeyword },
                { "COALESCE", KeywordType.FunctionKeyword },
                { "COLLATIONPROPERTY", KeywordType.FunctionKeyword },
                { "COLLECTION", KeywordType.OtherKeyword },
                { "COLUMN", KeywordType.OtherKeyword },
                { "COLUMNPROPERTY", KeywordType.FunctionKeyword },
                { "COL_LENGTH", KeywordType.FunctionKeyword },
                { "COL_NAME", KeywordType.FunctionKeyword },
                { "COMMIT", KeywordType.OtherKeyword },
                { "COMMITTED", KeywordType.OtherKeyword },
                { "COMPUTE", KeywordType.OtherKeyword },
                { "CONCAT", KeywordType.OtherKeyword },
                { "CONCAT_NULL_YIELDS_NULL", KeywordType.OtherKeyword },
                { "CONCURRENCYVIOLATION", KeywordType.OtherKeyword },
                { "CONFIRM", KeywordType.OtherKeyword },
                { "CONSTRAINT", KeywordType.OtherKeyword },
                { "CONTAINS", KeywordType.OtherKeyword },
                { "CONTAINSTABLE", KeywordType.FunctionKeyword },
                { "CONTINUE", KeywordType.OtherKeyword },
                { "CONTROL", KeywordType.OtherKeyword },
                { "CONTROLROW", KeywordType.OtherKeyword },
                { "CONVERT", KeywordType.FunctionKeyword },
                { "COS", KeywordType.FunctionKeyword },
                { "COT", KeywordType.FunctionKeyword },
                { "COUNT", KeywordType.FunctionKeyword },
                { "COUNT_BIG", KeywordType.FunctionKeyword },
                { "CREATE", KeywordType.OtherKeyword },
                { "CROSS", KeywordType.OtherKeyword },
                { "CURRENT", KeywordType.OtherKeyword },
                { "CURRENT_DATE", KeywordType.OtherKeyword },
                { "CURRENT_TIME", KeywordType.OtherKeyword },
                { "CURRENT_TIMESTAMP", KeywordType.FunctionKeyword },
                { "CURRENT_USER", KeywordType.FunctionKeyword },
                { "CURSOR", KeywordType.OtherKeyword },
                { "CURSOR_CLOSE_ON_COMMIT", KeywordType.OtherKeyword },
                { "CURSOR_STATUS", KeywordType.FunctionKeyword },
                { "DATABASE", KeywordType.OtherKeyword },
                { "DATABASEPROPERTY", KeywordType.FunctionKeyword },
                { "DATABASEPROPERTYEX", KeywordType.FunctionKeyword },
                { "DATALENGTH", KeywordType.FunctionKeyword },
                { "DATEADD", KeywordType.FunctionKeyword },
                { "DATEDIFF", KeywordType.FunctionKeyword },
                { "DATEFIRST", KeywordType.OtherKeyword },
                { "DATEFORMAT", KeywordType.OtherKeyword },
                { "DATENAME", KeywordType.FunctionKeyword },
                { "DATE", KeywordType.DataTypeKeyword },
                { "DATEPART", KeywordType.FunctionKeyword },
                { "DATETIME", KeywordType.DataTypeKeyword },
                { "DATETIME2", KeywordType.DataTypeKeyword },
                { "DATETIMEOFFSET", KeywordType.DataTypeKeyword },
                { "DAY", KeywordType.FunctionKeyword },
                { "DBCC", KeywordType.OtherKeyword },
                { "DBREINDEX", KeywordType.OtherKeyword },
                { "DBREPAIR", KeywordType.OtherKeyword },
                { "DB_ID", KeywordType.FunctionKeyword },
                { "DB_NAME", KeywordType.FunctionKeyword },
                { "DEADLOCK_PRIORITY", KeywordType.OtherKeyword },
                { "DEALLOCATE", KeywordType.OtherKeyword },
                { "DEC", KeywordType.DataTypeKeyword },
                { "DECIMAL", KeywordType.DataTypeKeyword },
                { "DECLARE", KeywordType.OtherKeyword },
                { "DEFAULT", KeywordType.OtherKeyword },
                { "DEFINITION", KeywordType.OtherKeyword },
                { "DEGREES", KeywordType.FunctionKeyword },
                { "DELAY", KeywordType.OtherKeyword },
                { "DELETE", KeywordType.OtherKeyword },
                { "DENY", KeywordType.OtherKeyword },
                { "DESC", KeywordType.OtherKeyword },
                { "DIFFERENCE", KeywordType.FunctionKeyword },
                { "DISABLE_DEF_CNST_CHK", KeywordType.OtherKeyword },
                { "DISK", KeywordType.OtherKeyword },
                { "DISTINCT", KeywordType.OtherKeyword },
                { "DISTRIBUTED", KeywordType.OtherKeyword },
                { "DOUBLE", KeywordType.DataTypeKeyword },
                { "DROP", KeywordType.OtherKeyword },
                { "DROPCLEANBUFFERS", KeywordType.OtherKeyword },
                { "DUMMY", KeywordType.OtherKeyword },
                { "DUMP", KeywordType.OtherKeyword },
                { "DYNAMIC", KeywordType.OtherKeyword },
                { "ELSE", KeywordType.OtherKeyword },
                { "ENCRYPTION", KeywordType.OtherKeyword },
                { "ERRLVL", KeywordType.OtherKeyword },
                { "ERROREXIT", KeywordType.OtherKeyword },
                { "ESCAPE", KeywordType.OtherKeyword },
                { "EXCEPT", KeywordType.OtherKeyword },
                { "EXEC", KeywordType.OtherKeyword },
                { "EXECUTE", KeywordType.OtherKeyword },
                { "EXISTS", KeywordType.OperatorKeyword },
                { "EXIT", KeywordType.OtherKeyword },
                { "EXP", KeywordType.FunctionKeyword },
                { "EXPAND", KeywordType.OtherKeyword },
                { "EXTERNAL", KeywordType.OtherKeyword },
                { "FAST", KeywordType.OtherKeyword },
                { "FAST_FORWARD", KeywordType.OtherKeyword },
                { "FASTFIRSTROW", KeywordType.OtherKeyword },
                { "FETCH", KeywordType.OtherKeyword },
                { "FILE", KeywordType.OtherKeyword },
                { "FILEGROUPPROPERTY", KeywordType.FunctionKeyword },
                { "FILEGROUP_ID", KeywordType.FunctionKeyword },
                { "FILEGROUP_NAME", KeywordType.FunctionKeyword },
                { "FILEPROPERTY", KeywordType.FunctionKeyword },
                { "FILE_ID", KeywordType.FunctionKeyword },
                { "FILE_IDEX", KeywordType.FunctionKeyword },
                { "FILE_NAME", KeywordType.FunctionKeyword },
                { "FILLFACTOR", KeywordType.OtherKeyword },
                { "FIPS_FLAGGER", KeywordType.OtherKeyword },
                { "FLOAT", KeywordType.DataTypeKeyword },
                { "FLOOR", KeywordType.FunctionKeyword },
                { "FLOPPY", KeywordType.OtherKeyword },
                { "FMTONLY", KeywordType.OtherKeyword },
                { "FOR", KeywordType.OtherKeyword },
                { "FORCE", KeywordType.OtherKeyword },
                { "FORCED", KeywordType.OtherKeyword },
                { "FORCEPLAN", KeywordType.OtherKeyword },
                { "FOREIGN", KeywordType.OtherKeyword },
                { "FORMATMESSAGE", KeywordType.FunctionKeyword },
                { "FORWARD_ONLY", KeywordType.OtherKeyword },
                { "FREEPROCCACHE", KeywordType.OtherKeyword },
                { "FREESESSIONCACHE", KeywordType.OtherKeyword },
                { "FREESYSTEMCACHE", KeywordType.OtherKeyword },
                { "FREETEXT", KeywordType.OtherKeyword },
                { "FREETEXTTABLE", KeywordType.FunctionKeyword },
                { "FROM", KeywordType.OtherKeyword },
                { "FULL", KeywordType.OtherKeyword },
                { "FULLTEXT", KeywordType.OtherKeyword },
                { "FULLTEXTCATALOGPROPERTY", KeywordType.FunctionKeyword },
                { "FULLTEXTSERVICEPROPERTY", KeywordType.FunctionKeyword },
                { "FUNCTION", KeywordType.OtherKeyword },
                { "GEOGRAPHY", KeywordType.DataTypeKeyword },
                { "GETANCESTOR", KeywordType.FunctionKeyword },
                { "GETANSINULL", KeywordType.FunctionKeyword },
                { "GETDATE", KeywordType.FunctionKeyword },
                { "GETDESCENDANT", KeywordType.FunctionKeyword },
                { "GETLEVEL", KeywordType.FunctionKeyword },
                { "GETREPARENTEDVALUE", KeywordType.FunctionKeyword },
                { "GETROOT", KeywordType.FunctionKeyword },
                { "GLOBAL", KeywordType.OtherKeyword },
                { "GO", KeywordType.OtherKeyword },
                { "GOTO", KeywordType.OtherKeyword },
                { "GRANT", KeywordType.OtherKeyword },
                { "GROUP", KeywordType.OtherKeyword },
                { "GROUPING", KeywordType.FunctionKeyword },
                { "HASH", KeywordType.OtherKeyword },
                { "HAVING", KeywordType.OtherKeyword },
                { "HELP", KeywordType.OtherKeyword },
                { "HIERARCHYID", KeywordType.DataTypeKeyword },
                { "HOLDLOCK", KeywordType.OtherKeyword },
                { "HOST_ID", KeywordType.FunctionKeyword },
                { "HOST_NAME", KeywordType.FunctionKeyword },
                { "IDENTITY", KeywordType.FunctionKeyword },
                { "IDENTITYCOL", KeywordType.OtherKeyword },
                { "IDENTITY_INSERT", KeywordType.OtherKeyword },
                { "IDENT_CURRENT", KeywordType.FunctionKeyword },
                { "IDENT_INCR", KeywordType.FunctionKeyword },
                { "IDENT_SEED", KeywordType.FunctionKeyword },
                { "IF", KeywordType.OtherKeyword },
                { "IGNORE_CONSTRAINTS", KeywordType.OtherKeyword },
                { "IGNORE_TRIGGERS", KeywordType.OtherKeyword },
                { "IMAGE", KeywordType.DataTypeKeyword },
                { "IMPLICIT_TRANSACTIONS", KeywordType.OtherKeyword },
                { "IN", KeywordType.OperatorKeyword },
                { "INDEX", KeywordType.OtherKeyword },
                { "INDEXDEFRAG", KeywordType.OtherKeyword },
                { "INDEXKEY_PROPERTY", KeywordType.FunctionKeyword },
                { "INDEXPROPERTY", KeywordType.FunctionKeyword },
                { "INDEX_COL", KeywordType.FunctionKeyword },
                { "INNER", KeywordType.OtherKeyword },
                { "INPUTBUFFER", KeywordType.OtherKeyword },
                { "INSENSITIVE", KeywordType.DataTypeKeyword },
                { "INSERT", KeywordType.OtherKeyword },
                { "INT", KeywordType.DataTypeKeyword },
                { "INTEGER", KeywordType.DataTypeKeyword },
                { "INTERSECT", KeywordType.OtherKeyword },
                { "INTO", KeywordType.OtherKeyword },
                { "IO", KeywordType.OtherKeyword },
                { "IS", KeywordType.OtherKeyword },
                { "ISDATE", KeywordType.FunctionKeyword },
                { "ISDESCENDANTOF", KeywordType.FunctionKeyword },
                { "ISNULL", KeywordType.FunctionKeyword },
                { "ISNUMERIC", KeywordType.FunctionKeyword },
                { "ISOLATION", KeywordType.OtherKeyword },
                { "IS_MEMBER", KeywordType.FunctionKeyword },
                { "IS_SRVROLEMEMBER", KeywordType.FunctionKeyword },
                { "JOIN", KeywordType.OtherKeyword },
                { "KEEP", KeywordType.OtherKeyword },
                { "KEEPDEFAULTS", KeywordType.OtherKeyword },
                { "KEEPFIXED", KeywordType.OtherKeyword },
                { "KEEPIDENTITY", KeywordType.OtherKeyword },
                { "KEY", KeywordType.OtherKeyword },
                { "KEYSET", KeywordType.OtherKeyword },
                { "KILL", KeywordType.OtherKeyword },
                { "LANGUAGE", KeywordType.OtherKeyword },
                { "LEFT", KeywordType.FunctionKeyword },
                { "LEN", KeywordType.FunctionKeyword },
                { "LEVEL", KeywordType.OtherKeyword },
                { "LIKE", KeywordType.OperatorKeyword },
                { "LINENO", KeywordType.OtherKeyword },
                { "LOAD", KeywordType.OtherKeyword },
                { "LOCAL", KeywordType.OtherKeyword },
                { "LOCK_TIMEOUT", KeywordType.OtherKeyword },
                { "LOG", KeywordType.FunctionKeyword },
                { "LOG10", KeywordType.FunctionKeyword },
                { "LOGIN", KeywordType.OtherKeyword },
                { "LOOP", KeywordType.OtherKeyword },
                { "LOWER", KeywordType.FunctionKeyword },
                { "LTRIM", KeywordType.FunctionKeyword },
                { "MATCHED", KeywordType.OtherKeyword },
                { "MAX", KeywordType.FunctionKeyword },
                { "MAX_QUEUE_READERS", KeywordType.OtherKeyword },
                { "MAXDOP", KeywordType.OtherKeyword },
                { "MAXRECURSION", KeywordType.OtherKeyword },
                { "MERGE", KeywordType.OtherKeyword },
                { "MIN", KeywordType.FunctionKeyword },
                { "MIRROREXIT", KeywordType.OtherKeyword },
                { "MODIFY", KeywordType.FunctionKeyword },
                { "MONEY", KeywordType.DataTypeKeyword },
                { "MONTH", KeywordType.FunctionKeyword },
                { "MOVE", KeywordType.OtherKeyword },
                { "NAMES", KeywordType.OtherKeyword },
                { "NATIONAL", KeywordType.DataTypeKeyword },
                { "NCHAR", KeywordType.DataTypeKeyword },
                { "NEWID", KeywordType.FunctionKeyword },
                { "NEXT", KeywordType.OtherKeyword },
                { "NOCHECK", KeywordType.OtherKeyword },
                { "NOCOUNT", KeywordType.OtherKeyword },
                { "NODES", KeywordType.FunctionKeyword },
                { "NOEXEC", KeywordType.OtherKeyword },
                { "NOEXPAND", KeywordType.OtherKeyword },
                { "NOLOCK", KeywordType.OtherKeyword },
                { "NONCLUSTERED", KeywordType.OtherKeyword },
                { "NOT", KeywordType.OperatorKeyword },
                { "NOWAIT", KeywordType.OtherKeyword },
                { "NTEXT", KeywordType.DataTypeKeyword },
                { "NTILE", KeywordType.FunctionKeyword },
                { "NULL", KeywordType.OtherKeyword },
                { "NULLIF", KeywordType.FunctionKeyword },
                { "NUMERIC", KeywordType.DataTypeKeyword },
                { "NUMERIC_ROUNDABORT", KeywordType.OtherKeyword },
                { "NVARCHAR", KeywordType.DataTypeKeyword },
                { "OBJECTPROPERTY", KeywordType.FunctionKeyword },
                { "OBJECTPROPERTYEX", KeywordType.FunctionKeyword },
                { "OBJECT", KeywordType.OtherKeyword },
                { "OBJECT_ID", KeywordType.FunctionKeyword },
                { "OBJECT_NAME", KeywordType.FunctionKeyword },
                { "OF", KeywordType.OtherKeyword },
                { "OFF", KeywordType.OtherKeyword },
                { "OFFSETS", KeywordType.OtherKeyword },
                { "ON", KeywordType.OtherKeyword },
                { "ONCE", KeywordType.OtherKeyword },
                { "ONLY", KeywordType.OtherKeyword },
                { "OPEN", KeywordType.OtherKeyword },
                { "OPENDATASOURCE", KeywordType.OtherKeyword },
                { "OPENQUERY", KeywordType.FunctionKeyword },
                { "OPENROWSET", KeywordType.FunctionKeyword },
                { "OPENTRAN", KeywordType.OtherKeyword },
                { "OPTIMIZE", KeywordType.OtherKeyword },
                { "OPTIMISTIC", KeywordType.OtherKeyword },
                { "OPTION", KeywordType.OtherKeyword },
                { "OR", KeywordType.OperatorKeyword },
                { "ORDER", KeywordType.OtherKeyword },
                { "OUTER", KeywordType.OtherKeyword },
                { "OUT", KeywordType.OtherKeyword },
                { "OUTPUT", KeywordType.OtherKeyword },
                { "OUTPUTBUFFER", KeywordType.OtherKeyword },
                { "OVER", KeywordType.OtherKeyword },
                { "OWNER", KeywordType.OtherKeyword },
                { "PAGLOCK", KeywordType.OtherKeyword },
                { "PARAMETERIZATION", KeywordType.OtherKeyword },
                { "PARSE", KeywordType.FunctionKeyword },
                { "PARSENAME", KeywordType.FunctionKeyword },
                { "PARSEONLY", KeywordType.OtherKeyword },
                { "PARTITION", KeywordType.OtherKeyword },
                { "PATINDEX", KeywordType.FunctionKeyword },
                { "PERCENT", KeywordType.OtherKeyword },
                { "PERM", KeywordType.OtherKeyword },
                { "PERMANENT", KeywordType.OtherKeyword },
                { "PERMISSIONS", KeywordType.FunctionKeyword },
                { "PI", KeywordType.FunctionKeyword },
                { "PINTABLE", KeywordType.OtherKeyword },
                { "PIPE", KeywordType.OtherKeyword },
                { "PLAN", KeywordType.OtherKeyword },
                { "POWER", KeywordType.FunctionKeyword },
                { "PREPARE", KeywordType.OtherKeyword },
                { "PRIMARY", KeywordType.OtherKeyword },
                { "PRINT", KeywordType.OtherKeyword },
                { "PRIVILEGES", KeywordType.OtherKeyword },
                { "PROC", KeywordType.OtherKeyword },
                { "PROCCACHE", KeywordType.OtherKeyword },
                { "PROCEDURE", KeywordType.OtherKeyword },
                { "PROCEDURE_NAME", KeywordType.OtherKeyword },
                { "PROCESSEXIT", KeywordType.OtherKeyword },
                { "PROCID", KeywordType.OtherKeyword },
                { "PROFILE", KeywordType.OtherKeyword },
                { "PUBLIC", KeywordType.OtherKeyword },
                { "QUERY", KeywordType.FunctionKeyword },
                { "QUERY_GOVERNOR_COST_LIMIT", KeywordType.OtherKeyword },
                { "QUEUE", KeywordType.OtherKeyword },
                { "QUOTED_IDENTIFIER", KeywordType.OtherKeyword },
                { "QUOTENAME", KeywordType.FunctionKeyword },
                { "RADIANS", KeywordType.FunctionKeyword },
                { "RAISERROR", KeywordType.OtherKeyword },
                { "RAND", KeywordType.FunctionKeyword },
                { "READ", KeywordType.OtherKeyword },
                { "READCOMMITTED", KeywordType.OtherKeyword },
                { "READCOMMITTEDLOCK", KeywordType.OtherKeyword },
                { "READPAST", KeywordType.OtherKeyword },
                { "READTEXT", KeywordType.OtherKeyword },
                { "READUNCOMMITTED", KeywordType.OtherKeyword },
                { "READ_ONLY", KeywordType.OtherKeyword },
                { "REAL", KeywordType.DataTypeKeyword },
                { "RECOMPILE", KeywordType.OtherKeyword },
                { "RECONFIGURE", KeywordType.OtherKeyword },
                { "REFERENCES", KeywordType.OtherKeyword },
                { "REMOTE_PROC_TRANSACTIONS", KeywordType.OtherKeyword },
                { "REPEATABLE", KeywordType.OtherKeyword },
                { "REPEATABLEREAD", KeywordType.OtherKeyword },
                { "REPLACE", KeywordType.FunctionKeyword },
                { "REPLICATE", KeywordType.FunctionKeyword },
                { "REPLICATION", KeywordType.OtherKeyword },
                { "RESTORE", KeywordType.OtherKeyword },
                { "RESTRICT", KeywordType.OtherKeyword },
                { "RETURN", KeywordType.OtherKeyword },
                { "RETURNS", KeywordType.OtherKeyword },
                { "REVERSE", KeywordType.FunctionKeyword },
                { "REVERT", KeywordType.OtherKeyword },
                { "REVOKE", KeywordType.OtherKeyword },
                { "RIGHT", KeywordType.FunctionKeyword },
                { "ROBUST", KeywordType.OtherKeyword },
                { "ROLE", KeywordType.OtherKeyword },
                { "ROLLBACK", KeywordType.OtherKeyword },
                { "ROUND", KeywordType.FunctionKeyword },
                { "ROWCOUNT", KeywordType.OtherKeyword },
                { "ROWGUIDCOL", KeywordType.OtherKeyword },
                { "ROWLOCK", KeywordType.OtherKeyword },
                { "ROWVERSION", KeywordType.DataTypeKeyword },
                { "RTRIM", KeywordType.FunctionKeyword },
                { "RULE", KeywordType.OtherKeyword },
                { "SAVE", KeywordType.OtherKeyword },
                { "SCHEMA", KeywordType.OtherKeyword },
                { "SCHEMA_ID", KeywordType.FunctionKeyword },
                { "SCHEMA_NAME", KeywordType.FunctionKeyword },
                { "SCOPE_IDENTITY", KeywordType.FunctionKeyword },
                { "SCROLL", KeywordType.OtherKeyword },
                { "SCROLL_LOCKS", KeywordType.OtherKeyword },
                { "SELECT", KeywordType.OtherKeyword },
                { "SELF", KeywordType.OtherKeyword },
                { "SERIALIZABLE", KeywordType.OtherKeyword },
                { "SERVER", KeywordType.OtherKeyword },
                { "SERVERPROPERTY", KeywordType.FunctionKeyword },
                { "SESSIONPROPERTY", KeywordType.FunctionKeyword },
                { "SESSION_USER", KeywordType.FunctionKeyword },
                { "SET", KeywordType.OtherKeyword },
                { "SETUSER", KeywordType.OtherKeyword },
                { "SHOWCONTIG", KeywordType.OtherKeyword },
                { "SHOWPLAN_ALL", KeywordType.OtherKeyword },
                { "SHOWPLAN_TEXT", KeywordType.OtherKeyword },
                { "SHOW_STATISTICS", KeywordType.OtherKeyword },
                { "SHRINKDATABASE", KeywordType.OtherKeyword },
                { "SHRINKFILE", KeywordType.OtherKeyword },
                { "SHUTDOWN", KeywordType.OtherKeyword },
                { "SIGN", KeywordType.FunctionKeyword },
                { "SIMPLE", KeywordType.OtherKeyword },
                { "SIN", KeywordType.FunctionKeyword },
                { "SMALLDATETIME", KeywordType.DataTypeKeyword },
                { "SMALLINT", KeywordType.DataTypeKeyword },
                { "SMALLMONEY", KeywordType.DataTypeKeyword },
                { "SOME", KeywordType.OperatorKeyword },
                { "SOUNDEX", KeywordType.FunctionKeyword },
                { "SPACE", KeywordType.FunctionKeyword },
                { "SQLPERF", KeywordType.OtherKeyword },
                { "SQL_VARIANT", KeywordType.DataTypeKeyword },
                { "SQL_VARIANT_PROPERTY", KeywordType.FunctionKeyword },
                { "SQRT", KeywordType.FunctionKeyword },
                { "SQUARE", KeywordType.FunctionKeyword },
                { "STATE", KeywordType.OtherKeyword },
                { "STATISTICS", KeywordType.OtherKeyword },
                { "STATIC", KeywordType.OtherKeyword },
                { "STATS_DATE", KeywordType.FunctionKeyword },
                { "STATUS", KeywordType.OtherKeyword },
                { "STDEV", KeywordType.FunctionKeyword },
                { "STDEVP", KeywordType.FunctionKeyword },
                { "STOPLIST", KeywordType.OtherKeyword },
                { "STR", KeywordType.FunctionKeyword },
                { "STUFF", KeywordType.FunctionKeyword },
                { "SUBSTRING", KeywordType.FunctionKeyword },
                { "SUM", KeywordType.FunctionKeyword },
                { "SUSER_ID", KeywordType.FunctionKeyword },
                { "SUSER_NAME", KeywordType.FunctionKeyword },
                { "SUSER_SID", KeywordType.FunctionKeyword },
                { "SUSER_SNAME", KeywordType.FunctionKeyword },
                { "SYNONYM", KeywordType.OtherKeyword },
                { "SYSNAME", KeywordType.DataTypeKeyword },
                { "SYSTEM_USER", KeywordType.FunctionKeyword },
                { "TABLE", KeywordType.OtherKeyword },
                { "TABLOCK", KeywordType.OtherKeyword },
                { "TABLOCKX", KeywordType.OtherKeyword },
                { "TAN", KeywordType.FunctionKeyword },
                { "TAPE", KeywordType.OtherKeyword },
                { "TEMP", KeywordType.OtherKeyword },
                { "TEMPORARY", KeywordType.OtherKeyword },
                { "TEXT", KeywordType.DataTypeKeyword },
                { "TEXTPTR", KeywordType.FunctionKeyword },
                { "TEXTSIZE", KeywordType.OtherKeyword },
                { "TEXTVALID", KeywordType.FunctionKeyword },
                { "THEN", KeywordType.OtherKeyword },
                { "TIME", KeywordType.DataTypeKeyword }, //not strictly-speaking true, can also be keyword in WAITFOR TIME
                { "TIMESTAMP", KeywordType.DataTypeKeyword },
                { "TINYINT", KeywordType.DataTypeKeyword },
                { "TO", KeywordType.OtherKeyword },
                { "TOP", KeywordType.OtherKeyword },
                { "TOSTRING", KeywordType.FunctionKeyword },
                { "TRACEOFF", KeywordType.OtherKeyword },
                { "TRACEON", KeywordType.OtherKeyword },
                { "TRACESTATUS", KeywordType.OtherKeyword },
                { "TRAN", KeywordType.OtherKeyword },
                { "TRANSACTION", KeywordType.OtherKeyword },
                { "TRIGGER", KeywordType.OtherKeyword },
                { "TRUNCATE", KeywordType.OtherKeyword },
                { "TSEQUAL", KeywordType.OtherKeyword },
                { "TYPEPROPERTY", KeywordType.FunctionKeyword },
                { "TYPE_ID", KeywordType.FunctionKeyword },
                { "TYPE_NAME", KeywordType.FunctionKeyword },
                { "TYPE_WARNING", KeywordType.OtherKeyword },
                { "UNCOMMITTED", KeywordType.OtherKeyword },
                { "UNICODE", KeywordType.FunctionKeyword },
                { "UNION", KeywordType.OtherKeyword },
                { "UNIQUE", KeywordType.OtherKeyword },
                { "UNIQUEIDENTIFIER", KeywordType.DataTypeKeyword },
                { "UNKNOWN", KeywordType.OtherKeyword },
                { "UNPINTABLE", KeywordType.OtherKeyword },
                { "UPDATE", KeywordType.OtherKeyword },
                { "UPDATETEXT", KeywordType.OtherKeyword },
                { "UPDATEUSAGE", KeywordType.OtherKeyword },
                { "UPDLOCK", KeywordType.OtherKeyword },
                { "UPPER", KeywordType.FunctionKeyword },
                { "USE", KeywordType.OtherKeyword },
                { "USER", KeywordType.FunctionKeyword },
                { "USEROPTIONS", KeywordType.OtherKeyword },
                { "USER_ID", KeywordType.FunctionKeyword },
                { "USER_NAME", KeywordType.FunctionKeyword },
                { "USING", KeywordType.OtherKeyword },
                { "VALUE", KeywordType.FunctionKeyword },
                { "VALUES", KeywordType.OtherKeyword },
                { "VAR", KeywordType.FunctionKeyword },
                { "VARBINARY", KeywordType.DataTypeKeyword },
                { "VARCHAR", KeywordType.DataTypeKeyword },
                { "VARP", KeywordType.FunctionKeyword },
                { "VARYING", KeywordType.OtherKeyword },
                { "VIEW", KeywordType.OtherKeyword },
                { "VIEWS", KeywordType.OtherKeyword },
                { "WAITFOR", KeywordType.OtherKeyword },
                { "WHEN", KeywordType.OtherKeyword },
                { "WHERE", KeywordType.OtherKeyword },
                { "WHILE", KeywordType.OtherKeyword },
                { "WITH", KeywordType.OtherKeyword },
                { "WORK", KeywordType.OtherKeyword },
                { "WRITE", KeywordType.FunctionKeyword },
                { "WRITETEXT", KeywordType.OtherKeyword },
                { "XACT_ABORT", KeywordType.OtherKeyword },
                { "XLOCK", KeywordType.OtherKeyword },
                { "XML", KeywordType.DataTypeKeyword },
                { "YEAR", KeywordType.FunctionKeyword }
            };
        }

        public enum KeywordType
        {
            OperatorKeyword,
            FunctionKeyword,
            DataTypeKeyword,
            OtherKeyword
        }
    }
}
