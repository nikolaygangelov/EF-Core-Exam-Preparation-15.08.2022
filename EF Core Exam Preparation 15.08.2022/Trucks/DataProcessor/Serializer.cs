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
            //using Data Transfer Object Class to map it with despatchers
            XmlSerializer serializer = new XmlSerializer(typeof(ExportDespatchersDTO[]), new XmlRootAttribute("Despatchers"));

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //"using" automatically closes opened connections
            using var writer = new StringWriter(sb);

            var xns = new XmlSerializerNamespaces();

            //one way to display empty namespace in resulted file
            xns.Add(string.Empty, string.Empty);

            var despatchersAndTrucks = context.Despatchers
                .Where(d => d.Trucks.Any())
                .Select(d => new ExportDespatchersDTO
                {
                    //using identical properties in order to map successfully
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

            //Serialize method needs file, TextReader object and namespace to convert/map
            serializer.Serialize(writer, despatchersAndTrucks, xns);

            //explicitly closing connection in terms of reaching edge cases
            writer.Close();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }

        public static string ExportClientsWithMostTrucks(TrucksContext context, int capacity)
        {
            //turning needed info about clients into a collection using anonymous object
            //using less data
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

            //Serialize method needs object to convert/map
	        //adding Formatting for better reading
            return JsonConvert.SerializeObject(clientsAndTrucks, Formatting.Indented);
        }
    }
}
