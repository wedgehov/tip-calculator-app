# Frontend Mentor - Tip calculator app solution

This is a solution to the [Tip calculator app challenge on Frontend Mentor](https://www.frontendmentor.io/challenges/tip-calculator-app-ugJNGbJUX). Frontend Mentor challenges help you improve your coding skills by building realistic projects.

## Table of contents

- [Overview](#overview)
  - [The challenge](#the-challenge)
  - [Screenshot](#screenshot)
  - [Links](#links)
- [Running Locally](#running-locally)
  - [With Docker (Recommended)](#with-docker-recommended)
  - [Without Docker](#without-docker)
- [My process](#my-process)
  - [Built with](#built-with)
  - [What I learned](#what-i-learned)
  - [Continued development](#continued-development)
  - [Useful resources](#useful-resources)
- [Author](#author)

## Overview

### The challenge

Your users should be able to:

- View the optimal layout for the app depending on their device's screen size
- See hover states for all interactive elements on the page
- Calculate the correct tip and total cost of the bill per person

### Screenshot

!

### Links

- Solution URL: Add solution URL here
- Live Site URL: Add live site URL here

## Running Locally

There are two ways to run this project: using Docker for a containerized environment, or running it directly on your machine.

### With Docker (Recommended)

This is the easiest way to get started, as it bundles all dependencies into a container.

1.  Ensure you have [Docker](https://www.docker.com/get-started) and [Docker Compose](https://docs.docker.com/compose/install/) installed.
2.  Build and run the container from the project root:
    ```bash
    docker-compose up --build
    ```
3.  Open your browser and navigate to `http://localhost:8080`.

### Without Docker

If you prefer to run the project on your local machine, you'll need to have Bun (v1.2.x or later) and the .NET 9 SDK installed.

1.  Clone the repository.
2.  Install dependencies:
    ```bash
    bun install
    ```
3.  Restore .NET local tools (which includes the Fable compiler):
    ```bash
    dotnet tool restore
    ```
4.  Start the Vite development server:
    ```bash
    bun run dev
    ```
5.  Open your browser and navigate to the local URL provided by Vite (usually `http://localhost:5173`).

## My process

This project was built using a functional-first approach with F# compiling to JavaScript.

### Built with

- **F#** - The primary language for application logic and UI.
- **The Elmish Architecture** - A Model-View-Update (MVU) pattern for predictable state management.
- **Feliz** - A React DSL (Domain-Specific Language) for building components in F#.
- **Fable** - The compiler that transpiles F# code into JavaScript.
- **React** - The underlying JavaScript library for building the user interface.
- **Vite** - The modern frontend build tool for development and production bundling.
- **Tailwind CSS** - A utility-first CSS framework for styling.
- **TypeScript** - Used for the main entry point to integrate with Vite.

### What I learned

This project was an excellent opportunity to apply a functional programming paradigm to a front-end application. Using F# with the Elmish architecture (MVU) made state management incredibly clear and predictable. All state changes flow through a single `update` function, which takes the current state and a message, and returns the new state. This eliminates a whole class of state-related bugs.

Feliz provides a beautiful, type-safe way to write React components. Instead of JSX, you write F# functions that map directly to React elements, giving you the full power of the F# type system to catch errors at compile time.

Here's a look at the core of the Elmish pattern in this app—the `update` function. It's a simple pattern match over the possible messages (user actions), making the application's logic easy to trace.

```fsharp
let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
  match msg with
  | BillChanged text ->
      { model with BillText = text } |> recalc, Cmd.none

  | PeopleChanged text ->
      { model with PeopleText = text } |> recalc, Cmd.none

  | SelectPresetTip pct ->
      { model with Tip = Some (Preset pct) } |> recalc, Cmd.none

  // ... and so on for other messages

  | Reset ->
      fst (init ()), Cmd.none
```

### Continued development

In the future, I'd like to continue improving this project by:

- **Adding more robust form validation:** While there is basic handling for zero people, I could add more sophisticated client-side validation to prevent non-numeric or negative inputs.
- **Implementing unit tests:** The core calculation logic in the `recalc` function is a perfect candidate for unit testing to ensure its correctness under all conditions.
- **Refactoring the view:** The main `view` function could be broken down into smaller, more manageable sub-views for better organization as the UI grows.

### Useful resources

- Fable - The F# to JavaScript compiler that makes all of this possible.
- Elmish - An excellent guide to the Elm Architecture in F#.
- Feliz - Documentation for the F# React DSL used to build the UI.
- Tailwind CSS - The official documentation for the utility-first CSS framework.

## Author

- Website - Add your name here
- Frontend Mentor - @yourusername
- Twitter - @yourusername
