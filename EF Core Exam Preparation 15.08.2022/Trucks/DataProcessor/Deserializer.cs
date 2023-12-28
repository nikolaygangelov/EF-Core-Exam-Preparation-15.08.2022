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
            //using Data Transfer Object Class to map it with Despatchers
            var serializer = new XmlSerializer(typeof(ImportDespatchersDTO[]), new XmlRootAttribute("Despatchers"));

            //Deserialize method needs TextReader object to convert/map 
            using StringReader inputReader = new StringReader(xmlString);
            var despatchersArrayDTOs = (ImportDespatchersDTO[])serializer.Deserialize(inputReader);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid despatchers can be kept
            List<Despatcher> despatchersXML = new List<Despatcher>();

            foreach (ImportDespatchersDTO despatcherDTO in despatchersArrayDTOs)
            {
                //validating info for despatcher from data
                if (!IsValid(despatcherDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //creating a valid despatcher
                Despatcher despatcherrToAdd = new Despatcher
                {
                    //using identical properties in order to map successfully
                    Name = despatcherDTO.Name,
                    Position = despatcherDTO.Position,
                };

                foreach (var truckDTO in despatcherDTO.Trucks)
                {
                    //validating info for truck from data
                    if (!IsValid(truckDTO))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //adding valid truck
                    despatcherrToAdd.Trucks.Add(new Truck()
                    {
                        //using identical properties in order to map successfully
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

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }
        public static string ImportClient(TrucksContext context, string jsonString)
        {
            //using Data Transfer Object Class to map it with clients
            var clientsArray = JsonConvert.DeserializeObject<ImportClientsDTO[]>(jsonString);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid clients can be kept
            List<Client> clientsList = new List<Client>();

            //taking only unique trucks
            var existingTrucksIds = context.Trucks
                .Select(tr => tr.Id)
                .ToArray();

            foreach (ImportClientsDTO clientDTO in clientsArray)
            {
                //validating info for client from data
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

                //creating a valid client
                Client clientToAdd = new Client()
                {
                    //using identical properties in order to map successfully
                    Name = clientDTO.Name,
                    Nationality = clientDTO.Nationality,
                    Type = clientDTO.Type
                };

                foreach (int truckId in clientDTO.ClientsTrucks.Distinct()) // 4, 17, 98
                {
                    //validating only unique trucks
                    if (!existingTrucksIds.Contains(truckId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //adding valid ClientTruck
                    clientToAdd.ClientsTrucks.Add(new ClientTruck()
                    {
                        Client = clientToAdd,
                        TruckId = truckId
                    });

                }

                clientsList.Add(clientToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedClient, clientToAdd.Name, clientToAdd.ClientsTrucks.Count));
            }

            context.AddRange(clientsList);

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
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
