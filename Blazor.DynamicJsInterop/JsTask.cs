using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

public class JsTask : DynamicObject {
    private readonly DynamicJSBase _dynamicJSBase;
    private readonly Task<object> _taskToReceiveCurrentJsObject;

    internal JsTask(DynamicJSBase dynamicJSBase, Task<object> taskToReceiveCurrentJsObject) {
        _dynamicJSBase = dynamicJSBase;
        _taskToReceiveCurrentJsObject = taskToReceiveCurrentJsObject;
    }
    
    public TaskAwaiter<object> GetAwaiter() {
       return _taskToReceiveCurrentJsObject.GetAwaiter();
    }
    
    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        async Task<object> GetValue() {
            var dynJs = new DynamicJSObjectReference((IJSObjectReference)await _taskToReceiveCurrentJsObject, _dynamicJSBase.JSRuntime,
                _dynamicJSBase.Options, _dynamicJSBase.AssemblyNameResolver);
            
            dynJs.TryGetMember(binder, out var res);
            if (res is JsTask jsTask) 
                return await jsTask;

            throw new Exception("Return Value needs to be of type JsTask");
        }
        
        result = new JsTask(_dynamicJSBase, GetValue());
        return true;
    }
    
    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result) {
        async Task<object> GetValue() {
            var dynJs = new DynamicJSObjectReference((IJSObjectReference)await _taskToReceiveCurrentJsObject, _dynamicJSBase.JSRuntime,
                _dynamicJSBase.Options, _dynamicJSBase.AssemblyNameResolver);
            
            dynJs.TryInvokeMember(binder, args, out var res);
            if (res is JsTask jsTask) 
                return await jsTask;

            throw new Exception("Return Value needs to be of type JsTask");
        }
        
        result = new JsTask(_dynamicJSBase, GetValue());
        return true;
    }
    
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result) {
        async Task<object> GetValue() {
            var dynJs = new DynamicJSObjectReference((IJSObjectReference)await _taskToReceiveCurrentJsObject, _dynamicJSBase.JSRuntime,
                _dynamicJSBase.Options, _dynamicJSBase.AssemblyNameResolver);
            
            dynJs.TryGetIndex(binder, indexes, out var res);
            if (res is JsTask jsTask) 
                return await jsTask;

            throw new Exception("Return Value needs to be of type JsTask");
        }
        
        result = new JsTask(_dynamicJSBase, GetValue());
        return true;
    }
}