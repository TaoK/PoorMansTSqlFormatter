/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011 Tao Klerks

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

using System;
using System.Xml;
using System.Configuration;
using System.Collections.Specialized;

namespace PoorMansTSqlFormatterNppPlugin
{
    class SettingsProviderCustomPathUnversioned : SettingsProvider, IApplicationSettingsProvider
    {
        /*
         * This is a quick replacement for the standard "LocalFileSettingsProvider" that allows you to 
         * explicitly specify the location of the file. 
         * It is specifically intended to be used by plugins that are expected to keep their settings 
         * in a well-defined area (defined by the hosting application).
         * 
         * The absolute minimum functionality is implemented, with no notion of versions or upgrading.
         */

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(this.ApplicationName, config);
        }

        public override string ApplicationName
        {
            get
            {
                return (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            }
            set
            {
                //Do nothing
            }
        }

        string _settingsPath = null;
        private string SettingsPath
        {
            get
            {
                if (_settingsPath == null)
                    throw new Exception("Settings path must be set in a \"settingsPath\" context entry in the Settings object before any data is loaded or saved!");
                return _settingsPath;
            }
            set
            {
                if (value != null)
                {
                    if (_settingsPath != null && value != _settingsPath)
                        throw new Exception("Settings path cannot be changed once it is set!");
                    _settingsPath = value;
                }
            }
        }

        private XmlDocument _currentSettingsDoc;
        private XmlDocument CurrentSettingsDocument
        {
            get
            {
                lock (this)
                {
                    if (_currentSettingsDoc == null)
                    {
                        _currentSettingsDoc = GetSettingsDocument(SettingsPath);
                        if (_currentSettingsDoc == null)
                        {
                            _currentSettingsDoc = new XmlDocument();
                            _currentSettingsDoc.AppendChild(_currentSettingsDoc.CreateElement("Settings"));
                        }
                    }
                    return _currentSettingsDoc;
                }
            }
        }

        private XmlDocument GetSettingsDocument(string targetPath)
        {
            try
            {
                XmlDocument settingsDoc = new XmlDocument();
                settingsDoc.Load(targetPath);
                return settingsDoc;
            }
            catch
            {
                return null;
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            SettingsPath = context["settingsPath"].ToString();
            SettingsPropertyValueCollection outputValues = new SettingsPropertyValueCollection();
            foreach (SettingsProperty property in collection)
            {
                outputValues.Add(GetPropertyValue(CurrentSettingsDocument, property));
            }
            return outputValues;
        }

        private static SettingsPropertyValue GetPropertyValue(XmlDocument sourceDoc, SettingsProperty property)
        {
            SettingsPropertyValue value = new SettingsPropertyValue(property);
            value.IsDirty = false;

            XmlElement propertyNode = (XmlElement)sourceDoc.SelectSingleNode(string.Format("//Setting[@name = '{0}']", property.Name.Replace("'", "&apos;")));
            if (propertyNode != null)
                value.SerializedValue = propertyNode.InnerText;
            else
                value.SerializedValue = property.DefaultValue;

            return value;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            SettingsPath = context["settingsPath"].ToString();
            foreach (SettingsPropertyValue propertyValue in collection)
            {
                SetPropertyValue(CurrentSettingsDocument, propertyValue);
            }
            CurrentSettingsDocument.Save(SettingsPath);
        }

        private static void SetPropertyValue(XmlDocument settingsDocument, SettingsPropertyValue propertyValue)
        {
            //try to get the element
            XmlElement propertyNode = (XmlElement)settingsDocument.SelectSingleNode(string.Format("//Setting[@name = '{0}']", propertyValue.Name.Replace("'", "&apos;")));

            //if not exists, then create
            if (propertyNode == null)
            {
                propertyNode = settingsDocument.CreateElement("Setting");
                propertyNode.SetAttribute("name", propertyValue.Name);
                settingsDocument.DocumentElement.AppendChild(propertyNode);
            }

            //set the value!
            propertyNode.InnerText = propertyValue.SerializedValue.ToString();
        }

        #region IApplicationSettingsProvider Members

        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            throw new NotImplementedException();
        }

        public void Reset(SettingsContext context)
        {
            SettingsPath = context["settingsPath"].ToString();
            CurrentSettingsDocument.DocumentElement.RemoveAll();
            CurrentSettingsDocument.Save(SettingsPath);
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
