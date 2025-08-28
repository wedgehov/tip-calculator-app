module App

open Elmish
open Feliz
open Browser.Dom
open Fable.Core.JsInterop

// ---------- Icons ----------
module Icons =
    let check =
        Svg.svg [
            svg.xmlns "http://www.w3.org/2000/svg"
            svg.fill "none"
            svg.viewBox (0, 0, 24, 24)
            svg.strokeWidth 2.5
            svg.stroke "currentColor"
            svg.className "w-4 h-4"
            svg.children [
                Svg.path [
                    Interop.svgAttribute "stroke-linecap" "round"
                    Interop.svgAttribute "stroke-linejoin" "round"
                    svg.d "M4.5 12.75l6 6 9-13.5"
                ]
            ]
        ]

    let trash =
        Svg.svg [
            svg.xmlns "http://www.w3.org/2000/svg"
            svg.fill "none"
            svg.viewBox (0, 0, 24, 24)
            svg.strokeWidth 1.5
            svg.stroke "currentColor"
            svg.className "w-5 h-5"
            svg.children [
                Svg.path [
                    Interop.svgAttribute "stroke-linecap" "round"
                    Interop.svgAttribute "stroke-linejoin" "round"
                    svg.d "M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0"
                ]
            ]
        ]

// ---------- Elmish ----------
type Todo = { Id: int; Description: string; IsDone: bool }
type Model = { Todos: Todo list; Input: string; NextId: int }
type Msg =
  | UpdateInput of string
  | AddTodo
  | ToggleTodo of int
  | DeleteTodo of int

let init () : Model * Cmd<Msg> =
  {
    Todos = [
      { Id = 1; Description = "Learn F#"; IsDone = true }
      { Id = 2; Description = "Learn Fable"; IsDone = true }
      { Id = 3; Description = "Build something awesome"; IsDone = false }
    ]
    Input = ""
    NextId = 4
  }, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
  match msg with
  | UpdateInput str ->
      { model with Input = str }, Cmd.none

  | AddTodo ->
      if System.String.IsNullOrWhiteSpace model.Input then
        model, Cmd.none
      else
        let newTodo =
          { Id = model.NextId; Description = model.Input.Trim(); IsDone = false }
        { model with
            Input = ""
            Todos = model.Todos @ [ newTodo ]
            NextId = model.NextId + 1
        }, Cmd.none

  | ToggleTodo todoId ->
      let updated =
        model.Todos
        |> List.map (fun t -> if t.Id = todoId then { t with IsDone = not t.IsDone } else t)
      { model with Todos = updated }, Cmd.none

  | DeleteTodo todoId ->
      { model with Todos = model.Todos |> List.filter (fun t -> t.Id <> todoId) }, Cmd.none

// ---------- View ----------
let view (model: Model) (dispatch: Msg -> unit) =
  Html.main [
    prop.className "min-h-screen flex flex-col items-center justify-center bg-zinc-900 text-white font-sans p-6"
    prop.children [
      Html.div [
        prop.className "w-full max-w-lg space-y-6"
        prop.children [
          Html.h1 [
            prop.className "text-4xl font-bold tracking-tight text-center text-zinc-200"
            prop.text "Feliz Todo App"
          ]
          Html.div [
            prop.className "rounded-2xl border border-zinc-800 bg-zinc-900/50 p-6 shadow-lg space-y-6 backdrop-blur-sm"
            prop.children [
              // Input form
              Html.form [
                prop.className "flex gap-3"
                prop.onSubmit (fun ev -> ev.preventDefault(); dispatch AddTodo)
                prop.children [
                  Html.input [
                    prop.className "flex-1 px-4 py-2 rounded-lg border border-zinc-700 bg-transparent focus:outline-none focus:ring-2 focus:ring-zinc-500 transition-shadow"
                    prop.placeholder "What needs to be done?"
                    prop.value model.Input
                    prop.onChange (UpdateInput >> dispatch)
                  ]
                  Html.button [
                    prop.className "px-6 py-2 rounded-lg border border-zinc-700 bg-zinc-800 hover:bg-zinc-700 transition-colors"
                    prop.text "Add"
                  ]
                ]
              ]

              // Todo list
              Html.div [
                prop.className "max-h-96 overflow-y-auto pr-2"
                prop.children [
                  Html.ul [
                    prop.className "space-y-3"
                    prop.children [
                      if List.isEmpty model.Todos then
                        Html.li [
                          prop.className "text-center text-zinc-500 py-4"
                          prop.text "All done! ✨"
                        ]
                      else
                        for todo in model.Todos do
                          Html.li [
                            prop.key todo.Id
                            prop.className "flex items-center gap-4 p-4 rounded-lg bg-zinc-800/50 transition-all hover:bg-zinc-800/70"
                            prop.children [
                              // Checkbox-like button
                              Html.button [
                                prop.classes [
                                  "w-6 h-6 rounded-full border-2 flex items-center justify-center transition-all"
                                  if todo.IsDone then "border-green-500 bg-green-500" else "border-zinc-600"
                                ]
                                prop.onClick (fun _ -> dispatch (ToggleTodo todo.Id))
                                prop.children [
                                  if todo.IsDone then Icons.check
                                ]
                              ]
                              // Description
                              Html.span [
                                prop.classes [
                                  "flex-1"
                                  if todo.IsDone then "line-through text-zinc-500" else "text-zinc-200"
                                ]
                                prop.text todo.Description
                              ]
                              // Delete button
                              Html.button [
                                prop.className "ml-auto text-zinc-500 hover:text-red-500 transition-colors"
                                prop.onClick (fun _ -> dispatch (DeleteTodo todo.Id))
                                prop.children [
                                  Icons.trash
                                ]
                              ]
                            ]
                          ]
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
  ]

// ---------- React 18 mount WITHOUT Elmish.React ----------
let reactDomClient = importAll<obj> "react-dom/client"

let start () =
  let container = document.getElementById "root"
  let root = reactDomClient?createRoot(container)
  Program.mkProgram init update view
  |> Program.withSetState (fun model dispatch ->
       root?render(view model dispatch))
  |> Program.run

do start ()
