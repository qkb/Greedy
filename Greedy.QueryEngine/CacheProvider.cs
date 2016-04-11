﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.QueryEngine
{
    public interface ICacheProvider
    {
        void Set(string name, object obj);
        void Set(string name, object obj, int expireTime);
        object Get(string name);
        void Set<T>(string name, T obj);
        void Set<T>(string name, T obj, int expireTime);
        T Get<T>(string name);
    }
}
