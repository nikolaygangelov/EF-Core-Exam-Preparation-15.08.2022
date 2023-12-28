namespace Trucks.DataProcessor
{
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System.Text;
    using System.Xml.Serialization;
    using Trucks.DataProcessor.ExportDto;

    public class Serializer
    {
        public static string ExportDespatchersWithTheirTrucks(TrucksContext context)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ExportDespatchersDTO[]), new XmlRootAttribute("Despatchers"));

            StringBuilder sb = new StringBuilder();

            using var writer = new StringWriter(sb);

            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            var despatchersAndTrucks = context.Despatchers
                .Where(d => d.Trucks.Any())
                .Select(d => new ExportDespatchersDTO
                {
                    TrucksCount = d.Trucks.Count,
                    Name = d.Name,
                    Trucks = d.Trucks
                    .Select(t => new ExportDespatcherTrucksDTO
                    {
                        RegistrationNumber = t.RegistrationNumber,
                        MakeType = t.MakeType
                    })
                    .OrderBy(t => t.RegistrationNumber)
                    .ToArray()
                })
                .OrderByDescending(d => d.TrucksCount)
                .ThenBy(d => d.Name)
                .ToArray();

            serializer.Serialize(writer, despatchersAndTrucks, xns);
            writer.Close();

            return sb.ToString();
        }

        public static string ExportClientsWithMostTrucks(TrucksContext context, int capacity)
        {
            var clientsAndTrucks = context.Clients
                .Include(c => c.ClientsTrucks)
                .ThenInclude(ct => ct.Truck)
                .ToArray()
                .Where(c => c.ClientsTrucks.Any(ct => ct.Truck.TankCapacity >= capacity))
                .Select(c => new
                {
                    Name = c.Name,
                    Trucks = c.ClientsTrucks
                    .Where(ct => ct.Truck.TankCapacity >= capacity)
                    .Select(ct => new
                    {
                        TruckRegistrationNumber = ct.Truck.RegistrationNumber,
                        VinNumber = ct.Truck.VinNumber,
                        TankCapacity = ct.Truck.TankCapacity,
                        CargoCapacity = ct.Truck.CargoCapacity,
                        CategoryType = ct.Truck.CategoryType.ToString(),
                        MakeType = ct.Truck.MakeType.ToString(),
                    })
                    .OrderBy(ct => ct.MakeType)
                    .ThenByDescending(ct => ct.CargoCapacity)
                    .ToArray()
                })
                .OrderByDescending(c => c.Trucks.Count())
                .ThenBy(c => c.Name)
                .Take(10);

            return JsonConvert.SerializeObject(clientsAndTrucks, Formatting.Indented);
        }
    }
}
