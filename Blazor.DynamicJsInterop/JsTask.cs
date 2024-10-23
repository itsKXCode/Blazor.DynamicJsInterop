using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

/// <summary>
/// Represents a Task which receives a JavaScript Property
/// </summary>
/// <typeparam name="T"></typeparam>
public class JSTask<T> : DynamicObject {
    private readonly DynamicJSBase _dynamicJSBase;
    protected readonly ValueTask<T> TaskToReceiveCurrentJsObject;

    internal JSTask(DynamicJSBase dynamicJSBase, ValueTask<T> taskToReceiveCurrentJsObject) {
        _dynamicJSBase = dynamicJSBase;
        TaskToReceiveCurrentJsObject = taskToReceiveCurrentJsObject;
    }
    
    public ValueTaskAwaiter<T> GetAwaiter() {
       return TaskToReceiveCurrentJsObject.GetAwaiter();
    }
    
    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        async ValueTask<object> GetValue() {
            var dynJs = new DynamicJSObjectReference(
                (IJSObjectReference)await TaskToReceiveCurrentJsObject, 
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
                (IJSObjectReference)await TaskToReceiveCurrentJsObject, 
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
                (IJSObjectReference)await TaskToReceiveCurrentJsObject, 
                _dynamicJSBase.JSRuntime,
                _dynamicJSBase.Options, 
                _dynamicJSBase.AssemblyNameResolver);
            
            dynJs.TryGetIndex(binder, indexes, out var res);
            return await (dynamic)res!;

        }
        
        result = new JSTask<object>(_dynamicJSBase, GetValue());
        return true;
    }
    public static implicit operator Task<T>(JSTask<T> task) => task.TaskToReceiveCurrentJsObject.AsTask();
    public static implicit operator ValueTask<T>(JSTask<T> task) => task.TaskToReceiveCurrentJsObject;
}

public class JSTask : JSTask<object> {
    internal JSTask(DynamicJSBase dynamicJSBase, ValueTask<object> taskToReceiveCurrentJsObject) 
        : base(dynamicJSBase, taskToReceiveCurrentJsObject) {
    }
    
    
    public static implicit operator Task(JSTask task) => task.TaskToReceiveCurrentJsObject.AsTask();
    public static implicit operator ValueTask(JSTask task) => new ValueTask(task.TaskToReceiveCurrentJsObject.AsTask());
}