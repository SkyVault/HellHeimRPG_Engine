namespace Functional

type Actor =
    { state: int }

type ActorStats =
    { health: int
      attack: float
      armor: float }

module Test =
    let greetFromFS name = name |> printfn "Hello %s from f#!"
