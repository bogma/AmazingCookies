// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake

// Directories
let buildDir  = "./build/"
let deployDir = "./deploy/"


// Filesets
let appReferences  =
    !! "/**/*.csproj"
      ++ "/**/*.fsproj"

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "ServerBuild" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "ClientBuild" (fun _ ->
    Shell.Exec ("elm-make", "app/Main.elm", "src/Client") |> ignore
    Copy buildDir [ "src/Client/index.html" ]
    ()
)

Target "DashboardBuild" (fun _ ->
    Shell.Exec ("elm-make", "app/Dashboard.elm --output dashboard.html", "src/Client") |> ignore
    Copy buildDir [ "src/Client/dashboard.html" ]
    ()
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "ApplicationName." + version + ".zip")
)

Target "RunElm" (fun _ ->
    let elm = tryFindFileOnPath "elm-make"

    let errorCode = match elm with
                      | Some elmmake -> Shell.Exec(elmmake, "app/Dashboard.elm --output dashboard.html", "src/Client")
                      | None -> -1

    //do something with the error code
    printf "The error code is %i" errorCode
    ()
)

// Build order
"Clean"
  ==> "ServerBuild"
  ==> "ClientBuild"
  ==> "DashboardBuild"
  ==> "Deploy"

// start build
RunTargetOrDefault "DashboardBuild"
