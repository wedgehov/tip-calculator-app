module App

open System
open System.Globalization
open Elmish
open Elmish.React
open Feliz

let logoUrl = "/images/logo.svg"
let dollarIconUrl = "/images/icon-dollar.svg"
let personIconUrl = "/images/icon-person.svg"

// --- Domain ---

type TipSelection =
  | Preset of int
  | Custom

type FieldState =
  | Pristine
  | Valid
  | Invalid of string

type Model =
  { BillText: string
    PeopleText: string
    Tip: TipSelection option
    CustomTipText: string
    BillState: FieldState
    PeopleState: FieldState
    CustomTipState: FieldState }

type Msg =
  | BillChanged of string
  | PeopleChanged of string
  | SelectPresetTip of int
  | SelectCustomTip
  | CustomTipChanged of string
  | Reset

// --- Init ---

let init () : Model * Cmd<Msg> =
  let model =
    { BillText = ""
      PeopleText = ""
      Tip = None
      CustomTipText = ""
      BillState = Pristine
      PeopleState = Pristine
      CustomTipState = Pristine }

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

let private validateBill (text: string) =
  let t = text.Trim()
  if t = "" then
    Pristine
  else
    match tryParseDecimal t with
    | Some v when v >= 0m -> Valid
    | Some _ -> Invalid "Can't be negative"
    | None -> Invalid "Invalid number"

let private validatePeople (text: string) =
  let t = text.Trim()
  if t = "" then
    Pristine
  else
    match tryParseInt t with
    | Some v when v > 0 -> Valid
    | Some 0 -> Invalid "Can't be zero"
    | Some _ -> Invalid "Can't be negative"
    | None -> Invalid "Invalid number"

let private validateCustomTip (text: string) =
  let t = text.Trim()
  if t = "" then
    Pristine
  else
    match tryParseDecimal t with
    | Some v when v >= 0m -> Valid
    | Some _ -> Invalid "Can't be negative"
    | None -> Invalid "Invalid number"

let private fieldHasError = function
  | Invalid _ -> true
  | _ -> false

let private fieldErrorText = function
  | Invalid msg -> Some msg
  | _ -> None

let private effectiveTipPercent (m: Model) =
  match m.Tip with
  | Some(Preset p) -> decimal p
  | Some Custom ->
      match m.CustomTipState, tryParseDecimal m.CustomTipText with
      | Valid, Some v -> v
      | _ -> 0m
  | None -> 0m

let private recalc (m: Model) =
  let billState = validateBill m.BillText
  let peopleState = validatePeople m.PeopleText

  let customTipState =
    match m.Tip with
    | Some Custom -> validateCustomTip m.CustomTipText
    | _ -> Pristine

  { m with
      BillState = billState
      PeopleState = peopleState
      CustomTipState = customTipState }

let private computeTotals (m: Model) : decimal * decimal =
  let bill =
    match m.BillState, tryParseDecimal m.BillText with
    | Valid, Some v -> v
    | _ -> 0m

  let people =
    match m.PeopleState, tryParseInt m.PeopleText with
    | Valid, Some v -> v
    | _ -> 0

  if people <= 0 then
    0m, 0m
  else
    let tipPct = (effectiveTipPercent m) / 100m
    let tipTotal = bill * tipPct
    let per = decimal people
    tipTotal / per, (bill + tipTotal) / per

// --- Update ---

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
  match msg with
  | BillChanged text ->
      { model with BillText = text } |> recalc, Cmd.none

  | PeopleChanged text ->
      { model with PeopleText = text } |> recalc, Cmd.none

  | SelectPresetTip pct ->
      { model with Tip = Some(Preset pct) } |> recalc, Cmd.none

  | SelectCustomTip ->
      { model with Tip = Some Custom } |> recalc, Cmd.none

  | CustomTipChanged text ->
      { model with Tip = Some Custom; CustomTipText = text } |> recalc, Cmd.none

  | Reset ->
      init ()

// --- View helpers ---

let tipButton (selected: bool) (label: string) (onClick: unit -> unit) =
  Html.button [
    prop.className
      ([ "w-full py-[6px] rounded-[5px] font-bold transition cursor-pointer"
         if selected then
           "bg-green-400 text-green-900"
         else
           "bg-teal-900 text-white hover:bg-green-200 hover:text-green-900" ]
       |> String.concat " ")
    prop.onClick (fun _ -> onClick ())
    prop.text label
  ]

let textInput
  (iconSrc: string option)
  (value: string)
  (placeholder: string)
  (onChange: string -> unit)
  (isError: bool)
  =
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
          [ "bg-gray-50 text-green-900 text-2xl font-bold text-right rounded-[5px] block w-full p-2.5 outline-none cursor-pointer" ]
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
    | Some(Preset p) -> Some p
    | _ -> None

  let customSelected = (model.Tip = Some Custom)
  let customHasError = customSelected && fieldHasError model.CustomTipState
  let customErrorText = fieldErrorText model.CustomTipState |> Option.defaultValue ""

  let tipPerPerson, totalPerPerson = computeTotals model

  let isPristine =
    model.BillText = ""
    && model.PeopleText = ""
    && model.CustomTipText = ""
    && model.Tip.IsNone

  Html.div [
    prop.className "min-h-screen bg-gray-200 flex flex-col items-center font-space-mono"
    prop.children [
      Html.img [
        prop.src logoUrl
        prop.alt "Logo"
        prop.className "mt-[50px] mb-[40.86px] lg:mt-[163px] lg:mb-[87.86px]"
      ]

      Html.div [
        prop.className "bg-white w-full max-w-[608px] lg:max-w-[920px] rounded-t-[25px] md:rounded-[25px] px-6 py-[34px] lg:py-8 grid lg:grid-cols-2 gap-8 md:gap-10 lg:gap-12 md:py-[54px] md:px-[75.5px] lg:px-[40px] md:mx-20 md:mb-20"
        prop.children [
          Html.div [
            prop.className "flex flex-col gap-8 mx-2 md:mx-0 md:gap-6 lg:my-[16.5px] lg:gap-10"
            prop.children [
              Html.div [
                Html.div [
                  prop.className "flex items-center justify-between mb-2"
                  prop.children [
                    Html.label [
                      prop.htmlFor "bill"
                      prop.className "text-gray-600"
                      prop.text "Bill"
                    ]
                    match fieldErrorText model.BillState with
                    | Some msg ->
                      Html.p [
                        prop.className "font-bold text-orange-400 text-sm"
                        prop.text msg
                      ]
                    | None -> Html.none
                  ]
                ]
                textInput
                  (Some dollarIconUrl)
                  model.BillText
                  "0"
                  (BillChanged >> dispatch)
                  (fieldHasError model.BillState)
              ]

              Html.div [
                Html.label [
                  prop.htmlFor "tip"
                  prop.className "text-gray-500 mb-2 block"
                  prop.text "Select Tip %"
                ]
                Html.div [
                  prop.className "grid grid-cols-2 md:grid-cols-3 gap-4"
                  prop.children (
                    [
                      Html.div [
                        prop.className "relative w-full"
                        prop.children [
                          Html.input [
                            prop.type' "text"
                            prop.value model.CustomTipText
                            prop.placeholder "Custom"
                            prop.onFocus (fun _ -> dispatch SelectCustomTip)
                            prop.onChange (CustomTipChanged >> dispatch)
                            prop.className (
                              [ "peer w-full py-[6px] rounded-[5px] font-bold transition text-center bg-gray-50 text-green-900 placeholder-gray-550 outline-none border-2 cursor-pointer" ]
                              @ (if customHasError then
                                   [ "border-orange-400 focus:border-orange-400" ]
                                 elif customSelected then
                                   [ "border-green-400 focus:border-green-400" ]
                                 else
                                   [ "border-transparent focus:border-green-400" ])
                              |> String.concat " ")
                          ]
                          if customHasError then
                            Html.div [
                              prop.className "absolute right-0 top-full mt-1 z-20 pointer-events-none opacity-0 translate-y-1 transition-all peer-hover:opacity-100 peer-hover:translate-y-0 peer-focus:opacity-100 peer-focus:translate-y-0"
                              prop.children [
                                Html.div [
                                  prop.className "relative rounded-md bg-orange-400 text-white text-xs font-bold px-2 py-1 shadow-lg whitespace-nowrap"
                                  prop.children [
                                    Html.span [ prop.text customErrorText ]
                                    Html.span [
                                      prop.className "absolute -top-1 right-3 h-2 w-2 rotate-45 bg-orange-400"
                                    ]
                                  ]
                                ]
                              ]
                            ]
                        ]
                      ]
                    ]
                    |> List.append (
                      [ 5; 10; 15; 25; 50 ]
                      |> List.map (fun p ->
                        tipButton
                          (selectedTip = Some p)
                          (sprintf "%d%%" p)
                          (fun () -> dispatch (SelectPresetTip p)))
                    )
                  )
                ]
              ]

              Html.div [
                Html.div [
                  prop.className "flex items-center justify-between mb-2"
                  prop.children [
                    Html.label [
                      prop.htmlFor "people"
                      prop.className "text-gray-600"
                      prop.text "Number of People"
                    ]
                    match fieldErrorText model.PeopleState with
                    | Some msg ->
                      Html.p [
                        prop.className "font-bold text-orange-400 text-sm"
                        prop.text msg
                      ]
                    | None -> Html.none
                  ]
                ]
                textInput
                  (Some personIconUrl)
                  model.PeopleText
                  "0"
                  (PeopleChanged >> dispatch)
                  (fieldHasError model.PeopleState)
              ]
            ]
          ]

          Html.div [
            prop.className "bg-green-900 rounded-[15px] px-[23px] md:px-[47.5px] lg:px-[40px] py-[29.5px] md:py-[43px] lg:py-[37.5px] md:p-8 flex flex-col justify-between text-white"
            prop.children [
              Html.div [
                prop.className "flex flex-col gap-6"
                prop.children [
                  Html.div [
                    prop.className "flex items-center justify-between"
                    prop.children [
                      Html.div [
                        Html.p [ prop.className "text-sm"; prop.text "Tip Amount" ]
                        Html.p [ prop.className "text-xs text-white/60"; prop.text "/ person" ]
                      ]
                      Html.p [
                        prop.className "text-3xl md:text-4xl font-bold text-green-400"
                        prop.text (sprintf "$%s" (formatAmount tipPerPerson))
                      ]
                    ]
                  ]

                  Html.div [
                    prop.className "flex items-center justify-between"
                    prop.children [
                      Html.div [
                        Html.p [ prop.className "text-sm"; prop.text "Total" ]
                        Html.p [ prop.className "text-xs text-white/60"; prop.text "/ person" ]
                      ]
                      Html.p [
                        prop.className "text-3xl md:text-4xl font-bold text-green-400"
                        prop.text (sprintf "$%s" (formatAmount totalPerPerson))
                      ]
                    ]
                  ]
                ]
              ]

              Html.div [
                prop.className "mt-8"
                prop.children [
                  Html.button [
                    prop.className "w-full py-3 rounded-lg font-bold bg-green-750 hover:bg-green-200 text-green-800 disabled:opacity-50 cursor-pointer"
                    prop.disabled isPristine
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
