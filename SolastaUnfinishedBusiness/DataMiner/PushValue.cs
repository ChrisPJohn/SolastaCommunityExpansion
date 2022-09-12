﻿#if DEBUG
using System;
using SolastaUnfinishedBusiness.Api.Infrastructure;

namespace SolastaUnfinishedBusiness.DataMiner
{
    public class PushValue<T> : Disposable
    {
        private readonly T oldValue;
        private Action<T> setValue;

        public PushValue(T value, Func<T> getValue, Action<T> setValue)
        {
            if (getValue == null) { throw new ArgumentNullException(nameof(getValue)); }

            this.setValue = setValue ?? throw new ArgumentNullException(nameof(setValue));
            oldValue = getValue();
            setValue(value);
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            setValue?.Invoke(oldValue);
            setValue = null;
        }

        #endregion
    }
}
#endif
