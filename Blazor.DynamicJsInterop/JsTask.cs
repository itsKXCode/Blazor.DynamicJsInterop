using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

public class JSTask<T> : DynamicObject {
    private readonly DynamicJSBase _dynamicJSBase;
    private readonly ValueTask<T> _taskToReceiveCurrentJsObject;

    internal JSTask(DynamicJSBase dynamicJSBase, ValueTask<T> taskToReceiveCurrentJsObject) {
        _dynamicJSBase = dynamicJSBase;
        _taskToReceiveCurrentJsObject = taskToReceiveCurrentJsObject;
    }
    
    public ValueTaskAwaiter<T> GetAwaiter() {
       return _taskToReceiveCurrentJsObject.GetAwaiter();
    }
    
    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        async ValueTask<object> GetValue() {
            var dynJs = new DynamicJSObjectReference(
                (IJSObjectReference)await _taskToReceiveCurrentJsObject, 
                _dynamicJSBase.JSRuntime,
                _dynamicJSBase.Options, 
                _dynamicJSBase.AssemblyNameResolver);
            
            dynJs.TryGetMember(binder, out var res);
            return await (dynamic)res!;
        }
        
        result = new JSTask<object>(_dynamicJSBase, GetValue());
        return true;
    }
    
    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result) {
        async ValueTask<object> GetValue() {
            var dynJs = new DynamicJSObjectReference(
                (IJSObjectReference)await _taskToReceiveCurrentJsObject, 
                _dynamicJSBase.JSRuntime,
                _dynamicJSBase.Options, 
                _dynamicJSBase.AssemblyNameResolver);
            
            dynJs.TryInvokeMember(binder, args, out var res);
            return await (dynamic)res!;

        }
        
        result = new JSTask<object>(_dynamicJSBase, GetValue());
        return true;
    }
    
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result) {
        async ValueTask<object> GetValue() {
            var dynJs = new DynamicJSObjectReference(
                (IJSObjectReference)await _taskToReceiveCurrentJsObject, 
                _dynamicJSBase.JSRuntime,
                _dynamicJSBase.Options, 
                _dynamicJSBase.AssemblyNameResolver);
            
            dynJs.TryGetIndex(binder, indexes, out var res);
            return await (dynamic)res!;

        }
        
        result = new JSTask<object>(_dynamicJSBase, GetValue());
        return true;
    }
}

public class JSTask : JSTask<object> {
    internal JSTask(DynamicJSBase dynamicJSBase, ValueTask<object> taskToReceiveCurrentJsObject) 
        : base(dynamicJSBase, taskToReceiveCurrentJsObject) {
    }
}