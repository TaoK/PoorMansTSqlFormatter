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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Reflection;

namespace PoorMansTSqlFormatterDemo.FrameworkClassReplacements
{
    class SingleAssemblyComponentResourceManager : System.ComponentModel //purposefully broken across 2 lines to avoid pre-build search/replace
        .ComponentResourceManager
    {
        private Type _contextTypeInfo;
        private CultureInfo _neutralResourcesCulture;

        public SingleAssemblyComponentResourceManager(Type t)
            : base(t)
        {
            _contextTypeInfo = t;
        }

        protected override ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            ResourceSet rs = (ResourceSet)this.ResourceSets[culture];
            if (rs == null)
            {
                Stream store = null;
                string resourceFileName = null;

                //lazy-load default language;
                if (this._neutralResourcesCulture == null)
                {
                    this._neutralResourcesCulture = GetNeutralResourcesLanguage(this.MainAssembly);
                }

                //if we're asking for the default language, then ask for the invaliant (non-specific) resources.
                if (_neutralResourcesCulture.Equals(culture))
                    culture = CultureInfo.InvariantCulture;
                resourceFileName = GetResourceFileName(culture);

                store = this.MainAssembly.GetManifestResourceStream(this._contextTypeInfo, resourceFileName);

                //If we found the appropriate resources in the local assembly
                if (store != null)
                {
                    rs = new ResourceSet(store);
                    //save for later.
                    AddResourceSet(this.ResourceSets, culture, ref rs);
                }
                else
                {
                    rs = base.InternalGetResourceSet(culture, createIfNotExists, tryParents);
                }
            }
            return rs;
        }

        //private method in framework, had to be re-specified
        private static void AddResourceSet(Hashtable localResourceSets, CultureInfo culture, ref ResourceSet rs)
        {
            lock (localResourceSets)
            {
                ResourceSet objA = (ResourceSet)localResourceSets[culture];
                if (objA != null)
                {
                    if (!object.Equals(objA, rs))
                    {
                        rs.Dispose();
                        rs = objA;
                    }
                }
                else
                {
                    localResourceSets.Add(culture, rs);
                }
            }
        }
    }
}
