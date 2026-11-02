#nullable disable // reference source kept as it is; stays oblivious where it is linked into nullable-enabled projects
//------------------------------------------------------------------------------
// <copyright file="JavaScriptTypeResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Serialization {
    using System;
    using System.Web;

    public abstract class JavaScriptTypeResolver {
        public abstract Type ResolveType(string id);
        public abstract string ResolveTypeId(Type type);
    }
}
