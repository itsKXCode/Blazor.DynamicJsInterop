Object.defineProperty(Object.prototype, "getPropertyType", {
        value: function getPropertyType(key) {

            //Represents the C# JsonValueKind Enum
            const JsonValueKind = {
                Undefined: 0,
                Object: 1,
                Array: 2,
                String: 3,
                Number: 4,
                True: 5,
                False: 6,
                Null: 7
            }

            const value = this[key];

            if (value === null)
                return JsonValueKind.Null;

            switch (typeof value) {
                case "object":
                    return Array.isArray(value) ? JsonValueKind.Array : JsonValueKind.Object;
                case "string":
                    return JsonValueKind.String;
                case "number":
                    return JsonValueKind.Number;
                case "boolean":
                    return value ? JsonValueKind.True : JsonValueKind.False;
                default:
                    return JsonValueKind.Undefined;
            }
        },
        writable: true,
        configurable: true
    });

Object.defineProperty(Object.prototype, "getProperty",
    {
        value: function getProperty(key) {
            return this[key];
        },
        writable: true,
        configurable: true
    });

Object.defineProperty(Object.prototype, "setProperty",
    {
        value: function setProperty(key, value) {
            this[key] = value;
        },
        writable: true,
        configurable: true
    });

Object.defineProperty(Object.prototype, "invokeMethodWrapped", {
        value: function invokeMethodWrapped(methodName, ...args) {
            const methodResult = this[methodName](...args);
            return {
                value: methodResult
            };
        },
        writable: true,
        configurable: true
    });

Object.defineProperty(Object.prototype, "invokeModuleMethodWrapped", {
        value: function invokeModuleMethodWrapped(targetInstanceId, identifier, ...args) {
            return new Promise(resolveMaster => {
                const promise = new Promise(resolve => {
                    const jsFunction = window.DotNet.findJSFunction(identifier, targetInstanceId);
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

Object.defineProperty(Object.prototype, "invokeMethod", {
    value: function invokeMethod(methodName, ...args) {
        return this[methodName](...args);
    },
    writable: true,
    configurable: true
});

Object.defineProperty(Object.prototype, "invokeModuleMethod", {
    value: function invokeModuleMethod(targetInstanceId, identifier, ...args) {
        return new Promise(resolveMaster => {
            const promise = new Promise(resolve => {
                const jsFunction = window.DotNet.findJSFunction(identifier, targetInstanceId);
                const synchronousResultOrPromise = jsFunction(...args);
                resolve(synchronousResultOrPromise);
            });

            promise.then(result => {
                    resolveMaster(result);
                }
            );
        })
    },
    writable: true,
    configurable: true
});