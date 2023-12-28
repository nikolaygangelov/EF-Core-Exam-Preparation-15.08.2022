namespace Trucks.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using Trucks.Data.Models;
    using Trucks.Data.Models.Enums;
    using Trucks.DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedDespatcher
            = "Successfully imported despatcher - {0} with {1} trucks.";

        private const string SuccessfullyImportedClient
            = "Successfully imported client - {0} with {1} trucks.";

        public static string ImportDespatcher(TrucksContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportDespatchersDTO[]), new XmlRootAttribute("Despatchers"));
            using StringReader inputReader = new StringReader(xmlString);
            var despatchersArrayDTOs = (ImportDespatchersDTO[])serializer.Deserialize(inputReader);

            StringBuilder sb = new StringBuilder();
            List<Despatcher> despatchersXML = new List<Despatcher>();

            foreach (ImportDespatchersDTO despatcherDTO in despatchersArrayDTOs)
            {
                Despatcher despatcherrToAdd = new Despatcher
                {
                    Name = despatcherDTO.Name,
                    Position = despatcherDTO.Position,
                };

                if (!IsValid(despatcherDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                foreach (var truckDTO in despatcherDTO.Trucks)
                {
                    if (!IsValid(truckDTO))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    despatcherrToAdd.Trucks.Add(new Truck()
                    {
                        RegistrationNumber = truckDTO.RegistrationNumber,
                        VinNumber = truckDTO.VinNumber,
                        TankCapacity = truckDTO.TankCapacity,
                        CargoCapacity = truckDTO.CargoCapacity,
                        CategoryType = (CategoryType)truckDTO.CategoryType,
                        MakeType = (MakeType)truckDTO.MakeType
                    });
                }

                despatchersXML.Add(despatcherrToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedDespatcher, despatcherrToAdd.Name,
                    despatcherrToAdd.Trucks.Count));
            }
            int despatchersCount = despatchersXML.Count();

            context.Despatchers.AddRange(despatchersXML);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }
        public static string ImportClient(TrucksContext context, string jsonString)
        {
            var clientsArray = JsonConvert.DeserializeObject<ImportClientsDTO[]>(jsonString);

            StringBuilder sb = new StringBuilder();
            List<Client> clientsList = new List<Client>();

            var existingTrucksIds = context.Trucks
                .Select(tr => tr.Id)
                .ToArray();

            foreach (ImportClientsDTO clientDTO in clientsArray)
            {

                if (!IsValid(clientDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (clientDTO.Type == "usual")
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Client clientToAdd = new Client()
                {
                    Name = clientDTO.Name,
                    Nationality = clientDTO.Nationality,
                    Type = clientDTO.Type
                };

                foreach (int truckId in clientDTO.ClientsTrucks.Distinct()) // 4, 17, 98
                {
                    if (!existingTrucksIds.Contains(truckId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    clientToAdd.ClientsTrucks.Add(new ClientTruck()
                    {
                        Client = clientToAdd,// !!!!!!!!!!!
                        TruckId = truckId
                    });

                }

                clientsList.Add(clientToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedClient, clientToAdd.Name, clientToAdd.ClientsTrucks.Count));
            }

            context.AddRange(clientsList);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}