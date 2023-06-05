﻿using Apps.Contentful.Authorization.OAuth2;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful
{
    public class ContentfulApplication : IApplication
    {
        private string _name;
        private readonly Dictionary<Type, object> _typesInstances;

        public ContentfulApplication()
        {
            _name = "Contentful";
            _typesInstances = CreateTypesInstances();
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public T GetInstance<T>()
        {
            if (!_typesInstances.TryGetValue(typeof(T), out var value))
            {
                throw new InvalidOperationException($"Instance of type '{typeof(T)}' not found");
            }
            return (T)value;
        }

        private Dictionary<Type, object> CreateTypesInstances()
        {
            return new Dictionary<Type, object>
            {
                { typeof(IOAuth2AuthorizeService), new OAuth2AuthorizeService() },
                { typeof(IOAuth2TokenService), new OAuth2TokenService() }
            };
        }
    }
}
