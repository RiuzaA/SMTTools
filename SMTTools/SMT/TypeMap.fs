module SMT.TypeMap

open System
open System.Collections.Generic
open System.Collections.Immutable

// A mutable dictionary of Types to values of those types
type TypeMap(dict: Dictionary<Type, obj>) =
    new() = TypeMap(new Dictionary<Type, obj>())

    member self.Set<'a> (a: 'a) = dict.[typedefof<'a>] <- a :> obj

    member self.Get<'a> () = dict.[typedefof<'a>] :?> 'a
    
    member self.Get<'a> (t: Type) = dict.[t] :?> 'a

    member self.GetOrCreate<'a> (f: unit -> 'a) =
        let key = typedefof<'a>
        if dict.ContainsKey key
        then (dict.[key] :?> 'a)
        else let newval = f ()
             dict.[key] <- newval :> obj
             newval

    member self.TryGet<'a> () =
        let key = typedefof<'a>
        if dict.ContainsKey key
        then Some (dict.[key] :?> 'a)
        else None

    member self.With<'a> (a: 'a) =
        self.Set a
        self

    member self.ToImmutable () = ImmutableTypeMap(dict.ToImmutableDictionary())

// An immutable dictionary of Types to values of those types
and ImmutableTypeMap(dict: ImmutableDictionary<Type, obj>) =
    new() = ImmutableTypeMap(ImmutableDictionary.Create ())

    member self.Set<'a> (a: 'a) : ImmutableTypeMap = ImmutableTypeMap(dict.Add(typedefof<'a>, a :> obj))

    member self.Get<'a> () = dict.[typedefof<'a>] :?> 'a

    member self.Get (t: Type) = dict.[t] :?> 'a

    member self.TryGet<'a> () =
        let key = typedefof<'a>
        if dict.ContainsKey key
        then Some (dict.[key] :?> 'a)
        else None

// A mutable dictionary of keys including Type to values of those types
type TypedMap<'k when 'k : equality>(dict: Dictionary<Type * 'k, obj>) =
    new() = TypedMap(new Dictionary<Type * 'k, obj>())

    member self.Set<'a> key (a: 'a) = dict.[(typedefof<'a>, key)] <- a :> obj

    member self.Get<'a> key = dict.[(typedefof<'a>, key)] :?> 'a
    
    member self.GetT<'a> (t: Type) key = dict.[(t, key)] :?> 'a

    member self.GetOrCreate<'a> key (f: unit -> 'a) =
        let key = typedefof<'a>, key
        if dict.ContainsKey key
        then (dict.[key] :?> 'a)
        else let newval = f ()
             dict.[key] <- newval :> obj
             newval

    member self.TryGetT<'a> (t: Type) key =
        let key = t, key
        if dict.ContainsKey key
        then Some (dict.[key] :?> 'a)
        else None

    member self.TryGet<'a> key = self.TryGetT<'a> typedefof<'a> key

    member self.With<'a> key (a: 'a) =
        self.Set key a
        self

    member self.Item
        with get key   = self.Get key
        and  set key v = self.Set key v

    member self.ToImmutable () = ImmutableTypedMap(dict.ToImmutableDictionary())
    
// An immutable dictionary of keys including Type to values of those types
and ImmutableTypedMap<'k when 'k : equality>(dict: ImmutableDictionary<Type * 'k, obj>) =
    new() = ImmutableTypedMap(ImmutableDictionary.Create ())

    member self.Set<'a> key (a: 'a) : ImmutableTypedMap<'k> = ImmutableTypedMap(dict.Add((typedefof<'a>, key), a :> obj))

    member self.Get<'a> key = dict.[(typedefof<'a>, key)] :?> 'a
    
    member self.GetT<'a> (t: Type) key = dict.[(t, key)] :?> 'a

    member self.TryGetT (t: Type) key =
        let key' = t, key
        if dict.ContainsKey key'
        then Some (dict.[key'] :?> 'a)
        else None

    member self.TryGet<'a> key = self.TryGetT<'a> typedefof<'a> key

    member self.Item
        with get idx = self.Get idx

module TypeMap =
    let withVal<'a> (a: 'a) (map: TypeMap) = map.With a

module ImmutableTypeMap =
    let ofMutable<'a> (map: TypeMap) = map.ToImmutable ()

module TypedMap =
    let withVal<'k, 'a when 'k : equality> (key: 'k) (a: 'a) (map: TypedMap<'k>) = map.With key a

module ImmutableTypedMap =
    let ofMutable<'k when 'k : equality> (map: TypedMap<'k>) = map.ToImmutable ()