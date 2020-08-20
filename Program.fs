module Server

open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared

type Storage() =
    let todos = ResizeArray<_>()

    member __.GetTodos() = List.ofSeq todos

    member __.AddTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new SAFE project edit")
|> ignore

storage.AddTodo(Todo.create "Write your app")
|> ignore

storage.AddTodo(Todo.create "Ship it !!!")
|> ignore

let getTodoHandler =
    fun next ctx -> json (storage.GetTodos()) next ctx

let todoHandler: HttpHandler =
    fun next ctx ->
        task {
            let! todo = ctx.BindModelAsync<Todo>()

            match storage.AddTodo todo with
            | Ok () -> return! json todo next ctx
            | Error e -> return! RequestErrors.BAD_REQUEST e next ctx
        }

let webApp =
    router {
        get Routes.todos getTodoHandler
        post Routes.todos todoHandler
    }

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
    }

run app
