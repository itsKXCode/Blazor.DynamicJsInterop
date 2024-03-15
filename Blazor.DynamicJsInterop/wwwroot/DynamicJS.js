Object.defineProperty(Object.prototype, "getPropertyAsObject",
    {
        value: function getPropertyAsObject(key)
        {
            return {
                Value: this[key]
            }
        },
        writable: true,
        configurable: true
    });

Object.defineProperty(Object.prototype, "getProperty",
    {
        value: function getProperty(key)
        {
            return this[key];
        },
        writable: true,
        configurable: true
    });

Object.defineProperty(Object.prototype, "setProperty",
    {
        value: function setProperty(key, value)
        {
            this[key] = value;
        },
        writable: true,
        configurable: true
    });

Object.defineProperty(Object.prototype, "invokeMethod",
    {
        value: function invokeMethod(methodName, ...args)
        {

            var methodResult = this[methodName](...args);
            return {
                value: methodResult
            };
        },
        writable: true,
        configurable: true
    });

Object.defineProperty(Object.prototype, "invokeModuleMethod",
    {
        value: function invokeModuleMethod(targetInstanceId, identifier, ...args)
        {
            return new Promise(resolveMaster => {
                const promise = new Promise(resolve => {
                    var jsFunction = window.DotNet.findJSFunction(identifier, targetInstanceId);
                    const synchronousResultOrPromise = jsFunction(...args);
                    resolve(synchronousResultOrPromise);
                });

                promise.then(result => {
                        resolveMaster({
                            value: result
                        });
                    }
                );
            })
        },
        writable: true,
        configurable: true
    });
