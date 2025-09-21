module App

open System
open System.Globalization
open Elmish
open Elmish.React
open Feliz
open Fable.Core.JsInterop

// Import assets to let Vite handle the paths
let logoUrl: string = importDefault "../public/images/logo.svg"
let dollarIconUrl: string = importDefault "../public/images/icon-dollar.svg"
let personIconUrl: string = importDefault "../public/images/icon-person.svg"

// ---------- Elmish ----------

// --- Domain ---

type TipSelection =
  | Preset of int       // 5 | 10 | 15 | 25 | 50
  | Custom

type Model =
  { BillText        : string
    PeopleText      : string
    Tip             : TipSelection option
    CustomTipText   : string
    TipPerPerson    : decimal
    TotalPerPerson  : decimal
    PeopleIsZero    : bool }

type Msg =
  | BillChanged        of string
  | PeopleChanged      of string
  | SelectPresetTip    of int
  | SelectCustomTip
  | CustomTipChanged   of string
  | Reset
  | Recalculate

// --- Init ---

let init () : Model * Cmd<Msg> =
  let model =
    { BillText       = ""
      PeopleText     = ""
      Tip            = None
      CustomTipText  = ""
      TipPerPerson   = 0m
      TotalPerPerson = 0m
      PeopleIsZero   = false }
  model, Cmd.none

// --- Helpers ---

let private tryParseDecimal (s: string) =
  match Decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture) with
  | true, v -> Some v
  | _ -> None

let private tryParseInt (s: string) =
  match Int32.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture) with
  | true, v -> Some v
  | _ -> None

let private formatAmount (d: decimal) =
  sprintf "%.2f" (float d)

let private effectiveTipPercent (m: Model) : decimal =
  match m.Tip with
  | Some (Preset p) -> decimal p
  | Some Custom ->
      match tryParseDecimal m.CustomTipText with
      | Some v when v >= 0m -> v
      | _ -> 0m
  | None -> 0m

let private recalc (m: Model) : Model =
  let bill =
    match tryParseDecimal m.BillText with
    | Some b when b >= 0m -> b
    | _ -> 0m

  let people =
    match tryParseInt m.PeopleText with
    | Some p when p >= 0 -> p
    | _ -> 0

  let tipPct = effectiveTipPercent m / 100m
  let peopleIsZero = (people = 0) && (m.PeopleText.Trim() <> "")

  if people <= 0 then
    { m with
        TipPerPerson = 0m
        TotalPerPerson = 0m
        PeopleIsZero = peopleIsZero }
  else
    let tipTotal = bill * tipPct
    let per = decimal people
    let tipPerPerson = if per > 0m then tipTotal / per else 0m
    let totalPerPerson = if per > 0m then (bill + tipTotal) / per else 0m
    { m with
        TipPerPerson = tipPerPerson
        TotalPerPerson = totalPerPerson
        PeopleIsZero = false }

// --- Update ---

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
  match msg with
  | BillChanged text ->
      { model with BillText = text } |> recalc, Cmd.none

  | PeopleChanged text ->
      { model with PeopleText = text } |> recalc, Cmd.none

  | SelectPresetTip pct ->
      { model with Tip = Some (Preset pct) } |> recalc, Cmd.none

  | SelectCustomTip ->
      { model with Tip = Some Custom } |> recalc, Cmd.none

  | CustomTipChanged text ->
      let m' =
        match model.Tip with
        | Some Custom -> model
        | _ -> { model with Tip = Some Custom }
      { m' with CustomTipText = text } |> recalc, Cmd.none

  | Reset ->
      fst (init ()), Cmd.none

  | Recalculate ->
      recalc model, Cmd.none

// --- View helpers ---

let tipButton (selected: bool) (label: string) (onClick: unit -> unit) =
  Html.button [
    prop.className
      ([ "w-full py-[6px] rounded-[5px] font-bold transition"
         if selected then
           "bg-green-400 text-green-900"
         else
           "bg-teal-900 text-white hover:bg-green-200 hover:text-green-900" ]
       |> String.concat " ")
    prop.onClick (fun _ -> onClick ())
    prop.text label
  ]

let textInput (iconSrc: string option) (value: string) (placeholder: string) (onChange: string -> unit) (isError: bool) =
  Html.div [
    prop.className "relative"
    prop.children [
      match iconSrc with
      | Some src ->
        Html.img [
          prop.src src
          prop.alt ""
          prop.className "absolute left-4 top-1/2 -translate-y-1/2"
        ]
      | None -> Html.none

      Html.input [
        prop.type' "text"
        prop.value value
        prop.placeholder placeholder
        prop.onChange onChange
        prop.className (
          [ "bg-gray-50 text-green-900 text-2xl font-bold text-right rounded-[5px] block w-full p-2.5 outline-none" ]
          @ (if isError then
               [ "border-2 border-orange-400 focus:border-orange-400" ]
             else
               [ "border-2 border-transparent focus:border-green-400" ])
          @ (if iconSrc.IsSome then [ "pl-12" ] else [])
          |> String.concat " ")
      ]
    ]
  ]

// --- View ---

let view (model: Model) (dispatch: Msg -> unit) =
  let selectedTip =
    match model.Tip with
    | Some (Preset p) -> Some p
    | Some Custom -> None
    | None -> None

  Html.div [
    prop.className "min-h-screen bg-gray-200 flex flex-col items-center"
    prop.children [

      Html.img [
        prop.src logoUrl
        prop.alt "Logo"
        prop.className "mt-[50px] mb-[40.86px] lg:mt-[163px] lg:mb-[87.86px]"
      ]

      Html.div [
        prop.className "bg-white w-full max-w-[608px] lg:max-w-[920px] rounded-t-[25px] md:rounded-[25px] px-6 py-[34px] lg:py-8 grid lg:grid-cols-2 gap-8 md:gap-10 lg:gap-12 md:py-[54px] md:px-[75.5px] lg:px-[40px] md:mx-20 md:mb-20"
        prop.children [

          // Left column: Inputs
          Html.div [
            prop.className "flex flex-col gap-8 mx-2 md:mx-0 md:gap-6 lg:my-[16.5px] lg:gap-10"

            prop.children [

              // Bill
              Html.div [
                Html.label [
                  prop.htmlFor "bill"
                  prop.className "text-gray-600 mb-2 block"
                  prop.text "Bill"
                ]
                textInput (Some dollarIconUrl) model.BillText "0" (BillChanged >> dispatch) false
              ]

              // Tip selection
              Html.div [
                Html.label [
                  prop.htmlFor "tip"
                  prop.className "text-gray-500 mb-2 block"
                  prop.text "Select Tip %"
                ]
                Html.div [
                  prop.className "grid grid-cols-2 md:grid-cols-3 gap-4"
                  prop.children [
                    [
                      tipButton (model.Tip = Some Custom) "Custom" (fun () -> dispatch SelectCustomTip)
                    ]
                    |> List.append (
                      [5;10;15;25;50]
                      |> List.map (fun p ->
                          tipButton (selectedTip = Some p)
                                    (sprintf "%d%%" p)
                                    (fun () -> dispatch (SelectPresetTip p))))
                      |> React.fragment 
                  ]
                ]

                // Custom input (only when Custom selected)
                match model.Tip with
                | Some Custom ->
                    Html.div [
                      prop.className "mt-3"
                      prop.children [
                        textInput None model.CustomTipText "Enter custom %" (CustomTipChanged >> dispatch) false
                      ]
                    ]
                | _ -> Html.none
              ]

              // Number of people
              Html.div [
                Html.div [
                  prop.className "flex items-center justify-between mb-2"
                  prop.children [
                    Html.label [
                      prop.htmlFor "people"
                      prop.className "text-gray-600"
                      prop.text "Number of People"
                    ]
                    if model.PeopleIsZero then
                      Html.p [
                        prop.className "font-bold text-orange-400 text-sm"
                        prop.text "Can't be zero"
                      ]
                    else Html.none
                  ]
                ]
                textInput (Some personIconUrl) model.PeopleText "0" (PeopleChanged >> dispatch) model.PeopleIsZero
              ]

            ]
          ]

          // Right column: Results
          Html.div [
            prop.className "bg-green-900 rounded-[15px] px-[23px] md:px-[47.5px] lg:px-[40px] py-[29.5px] md:py-[43px] lg:py-[37.5px] md:p-8 flex flex-col justify-between text-white"
            prop.children [

              Html.div [
                prop.className "flex flex-col gap-6"
                prop.children [

                  // Tip per person
                  Html.div [
                    prop.className "flex items-center justify-between"
                    prop.children [
                      Html.div [
                        Html.p [ prop.className "text-sm"; prop.text "Tip Amount" ]
                        Html.p [ prop.className "text-xs text-white/60"; prop.text "/ person" ]
                      ]
                      Html.p [
                        prop.className "text-3xl md:text-4xl font-bold text-green-400"
                        prop.text (sprintf "$%s" (formatAmount model.TipPerPerson))
                      ]
                    ]
                  ]

                  // Total per person
                  Html.div [
                    prop.className "flex items-center justify-between"
                    prop.children [
                      Html.div [
                        Html.p [ prop.className "text-sm"; prop.text "Total" ]
                        Html.p [ prop.className "text-xs text-white/60"; prop.text "/ person" ]
                      ]
                      Html.p [
                        prop.className "text-3xl md:text-4xl font-bold text-green-400"
                        prop.text (sprintf "$%s" (formatAmount model.TotalPerPerson))
                      ]
                    ]
                  ]
                ]
              ]

              Html.div [
                prop.className "mt-8"
                prop.children [
                  Html.button [
                    prop.className "w-full py-3 rounded-lg font-bold bg-green-750 hover:bg-green-200 text-green-800 disabled:opacity-50"
                    prop.disabled false
                    prop.onClick (fun _ -> dispatch Reset)
                    prop.text "RESET"
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
  ]

let start () =
  Program.mkProgram init update view
  |> Program.withReactBatched "root"
  |> Program.run

do start ()
