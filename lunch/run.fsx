#r "System.Net.Http"
#r "FSharp.Data.dll"

open FSharp.Data
open System
open System.Globalization
open System.Net
open System.Net.Http

type provider = JsonProvider<""" {"results": [{ "dag": "Day", "rett": "Dish" }] }""">

let Run(req: HttpRequestMessage) =
    async {
        let pair =
            req.GetQueryNameValuePairs()
            |> Seq.tryFind (fun q -> q.Key = "text" && q.Value <> "")

        let day =
            let cultureInfo = CultureInfo("nb-NO") 
            match pair with
            | Some x -> cultureInfo.TextInfo.ToTitleCase (x.Value.ToLower())
            | None -> cultureInfo.TextInfo.ToTitleCase (cultureInfo.DateTimeFormat.GetDayName(DateTime.Now.DayOfWeek))

        let dayDish =
            provider.Load("http://www.tibeapp.no/hosted/albatross/xlsx/test.php?fileurl=http://netpresenter.albatross-as.no/xlkantiner/Kanalsletta4.xlsx").Results
                |> Array.filter (fun item -> item.Dag = day) 
                |> Array.map (fun item -> item.Rett)  
                |> String.concat " "
                |> sprintf "%s: %s" day 

        return req.CreateResponse(HttpStatusCode.OK, dayDish)
    } |> Async.RunSynchronously