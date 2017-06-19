using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;

namespace CognitiveServicesFacialRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            train();
            Console.WriteLine("Done");
            Console.ReadLine();
        } 
        static async void train()
        {
            FaceServiceClient faceServiceClient = new FaceServiceClient("XXXXXXXXXXXXXXXX");

            // Create an empty person group
            string personGroupId = "test1";
            await faceServiceClient.CreatePersonGroupAsync(personGroupId, "Test 1");

            CreatePersonResult friend1 = await faceServiceClient.CreatePersonAsync(personGroupId, "Anna");
            CreatePersonResult friend2 = await faceServiceClient.CreatePersonAsync(personGroupId, "Bill");
            CreatePersonResult friend3 = await faceServiceClient.CreatePersonAsync(personGroupId, "Clare");

            // Directory contains image files of Anna
            const string friend1ImageDir = @"C:\Users\StanDotloe\OneDrive\Facial Recognition\Projects\Cognitive-Face-Windows-master\Data\PersonGroup\Family3-Lady";
            const string friend2ImageDir = @"C:\Users\StanDotloe\OneDrive\Facial Recognition\Projects\Cognitive-Face-Windows-master\Data\PersonGroup\Family1-Dad";
            const string friend3ImageDir = @"C:\Users\StanDotloe\OneDrive\Facial Recognition\Projects\Cognitive-Face-Windows-master\Data\PersonGroup\Family1-Mom";

            foreach (string imagePath in Directory.GetFiles(friend1ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceServiceClient.AddPersonFaceAsync(
                        personGroupId, friend1.PersonId, s);
                }
            }

            foreach (string imagePath in Directory.GetFiles(friend2ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    await faceServiceClient.AddPersonFaceAsync(personGroupId, friend2.PersonId, s);
                }
            }

            foreach (string imagePath in Directory.GetFiles(friend3ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    await faceServiceClient.AddPersonFaceAsync(personGroupId, friend3.PersonId, s);
                }
            }

            await faceServiceClient.TrainPersonGroupAsync(personGroupId);

            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (!(trainingStatus.Status.Equals("running")))
                {
                    break;
                }

                await Task.Delay(1000);
            }


            string testImageFile = @"C:\Users\StanDotloe\OneDrive\Facial Recognition\Projects\Cognitive-Face-Windows-master\Data\PersonGroup\Family3-Man\Family3-Man3.jpg";

            using (Stream s = File.OpenRead(testImageFile))
            {
                var faces = await faceServiceClient.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
                foreach (var identifyResult in results)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Length == 0)
                    {
                        Console.WriteLine("No one identified");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                        Console.WriteLine("Identified as {0}", person.Name);
                    }
                }
            }


        }
    }
}
