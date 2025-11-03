using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;




int intervalSec = 25;
if (Environment.GetEnvironmentVariable("SYNC_INTERVAL_SEC") is { } iv && int.TryParse(iv, out var parsed) && parsed > 0)
    intervalSec = parsed;

static T ExecuteWithRetry<T>(Func<T> op, int max = 3, int delayMs = 500)
{
    for (int attempt = 1; ; attempt++)
    {
        try { return op(); }
        catch (Exception ex) when (attempt < max)
        {
            Console.WriteLine($"[WARN] {DateTime.UtcNow:o}  {ex.Message} – retry {attempt}");
            Thread.Sleep(delayMs);
        }
    }
}

while (true)
{

    using var db = new MultiProtocolContext();
    var Clients = ExecuteWithRetry(() =>
        db.Client_Traffics
        .AsNoTracking()
        .Select(t => new Client_Traffics {
            Id = t.Id,
            Inbound_Id = t.Inbound_Id,
            Up = t.Up,
            Down = t.Down,
            Total = t.Total,
            Email = t.Email,
            Enable = t.Enable,
            Expiry_Time = t.Expiry_Time
        })
        .ToList());
    var localPath = Path.Combine(AppContext.BaseDirectory,"LocalDB.json");

    if (!File.Exists(localPath))
    {
        localDB local = new localDB() { Sec = 10, clients = Clients };
        var LocalD =File.Create(localPath);
        using( var writer = new StreamWriter(LocalD))
        {
            writer.Write(JsonConvert.SerializeObject(local));
        }
        LocalD.Close();
    }
    localDB localDB = JsonConvert.DeserializeObject<localDB>(File.ReadAllText(localPath));

   
    List<Client> ALLClients = new List<Client>();

    var inbounds = ExecuteWithRetry(() => db.Inbounds
        .AsNoTracking()
        .Select(i => new { i.Id, i.Protocol, i.Settings })
        .ToList());
    foreach (var item in inbounds)
    {
        if (string.IsNullOrWhiteSpace(item.Settings)) continue;
        var setting = JsonConvert.DeserializeObject<inboundsetting>(item.Settings!);
        if (setting?.clients != null)
        {
            ALLClients.AddRange(setting.clients);
        }
    }

    List<Client> FinalClients = new List<Client>();
    List<Client_Traffics> FinalClients_Traffic = new List<Client_Traffics>();
    List<Inbound> FinalInbounds = new List<Inbound>();

    foreach (var client in ALLClients)
    {
        if (!FinalClients.Any(x => x.subId == client.subId))
        {
            if (ALLClients.Where(x => x.subId == client.subId).Count() > 1)
            {

                List<Client> Calculate = ALLClients.Where(x => x.subId == client.subId).ToList();
                List<Client_Traffics> Calculate2 = new List<Client_Traffics>();
                foreach (var client2 in Calculate)
                {
                    Calculate2.Add(Clients.Where(x => x.Email == client2.email).FirstOrDefault());
                }

                Int64? maxTotalGB = Calculate.Max(x => x.totalGB);
                Int64? maxTotal = Calculate2.Max(x => x.Total);

                Int64? maxUP = Calculate2.Max(x => x.Up);
                Int64? maxDOWN = Calculate2.Max(x => x.Down);
                Int64? UP = 0;
                Int64? DOWN = 0;

                Int64? DateMax= Calculate2.Max(x => x.Expiry_Time);
                Int64? DateMin= Calculate2.Min(x => x.Expiry_Time);
                Int64? ExpireTime=0;
                if (DateMax > 0)
                {
                    ExpireTime = DateMax;
                }
                else if (DateMin < 0)
                    ExpireTime = DateMin;
                try
                {
                    foreach (var client2 in Calculate2)
                    {
                        
                        if (client2.Up != maxUP)
                        {
                            Int64? oldusage = localDB.clients.Where(x => x.Email == client2.Email).First().Up;
                            if (client2.Up > oldusage && oldusage != 0 && oldusage != null)
                                UP += client2.Up - oldusage;
                        }
                        if (client2.Down != maxDOWN)
                        {
                            Int64? oldusage = localDB.clients.Where(x => x.Email == client2.Email).First().Down;

                            if (client2.Down > oldusage && oldusage != 0 && oldusage != null)
                                DOWN += client2.Down - oldusage;
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.Message); }

                foreach (var cal2 in Calculate2)
                {
                    cal2.Total = maxTotal;
                    cal2.Up = maxUP+UP;
                    cal2.Down = maxDOWN+DOWN;
                    cal2.Expiry_Time = ExpireTime;
                    FinalClients_Traffic.Add(cal2);

                }
                foreach (var cal in Calculate)
                {
                    cal.totalGB = maxTotalGB;
                    cal.expiryTime = ExpireTime;
                    FinalClients.Add(cal);
                }
            }

        }

    }


    ExecuteWithRetry(() =>
    {
        db.Client_Traffics.UpdateRange(FinalClients_Traffic);
        db.Inbounds.UpdateRange(FinalInbounds);
        return db.SaveChanges();
    });

    try {
        foreach (var inbound in db.Inbounds)
        {
            if(inbound.Protocol== "vmess" || inbound.Protocol == "vless")
            {
                inboundsetting setting = JsonConvert.DeserializeObject<inboundsetting>(inbound.Settings);
                var clis = FinalClients_Traffic.Where(x => x.Inbound_Id == inbound.Id).ToList();
                List<Client> addtoInbound = new List<Client>();
                foreach (var client in clis)
                {
                    addtoInbound.Add(FinalClients.Where(x => x.email == client.Email).FirstOrDefault());

                }
                if (addtoInbound.Count() > 0)
                {

                    List<Client> pastclients = new List<Client>();
                    foreach (Client client in setting.clients)
                        if (!addtoInbound.Any(x => x.email == client.email)) { pastclients.Add(client); }
                    pastclients.AddRange(addtoInbound);
                    setting.clients = pastclients;
                    inbound.Settings = JsonConvert.SerializeObject(setting);
                    FinalInbounds.Add(inbound);
                }
            }
            
        }

    }
    catch (Exception e) { Console.WriteLine(e.Message); }
    db.Inbounds.UpdateRange(FinalInbounds);
    db.SaveChanges();
    var client_Traffics = new MultiProtocolContext().Client_Traffics
       .ToList();

     localDB updateLocal = new localDB() { Sec = localDB.Sec, clients = client_Traffics };
    File.Delete(localPath);
    var file = File.Create(localPath);
    StreamWriter streamWriter = new StreamWriter(file);
        streamWriter.Write(JsonConvert.SerializeObject(updateLocal));
        streamWriter.Close();
    file.Close();

    Console.WriteLine("Done");
    Thread.Sleep(intervalSec * 1000);

}






