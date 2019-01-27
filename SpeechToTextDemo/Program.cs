using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToTextDemo
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Prompts the user to input text for TTS conversion
            Console.WriteLine("Preparing to upload Lonely.wav...");

            var subscriptionKey = "61ecb661e3fc47568af25a145ac782d0";

            var host = "eastus.stt.speech.microsoft.com";
            var parameters = "?language=en-US&format=detailed";
            var requestUri =
                "https://eastus.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1" +
                parameters;

            var audioFile = "Lonely.wav";

            try
            {
                //await ChunkedSpeechToText(requestUri, host, subscriptionKey, audioFile);
                //await SimpleSpeechToText(requestUri, host, subscriptionKey, audioFile);
                await ShortSpeechToText(requestUri, host, subscriptionKey, audioFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static async Task ShortSpeechToText(string requestUri, string host, string subscriptionKey,
            string audioFile)
        {
            // Create a request
            Console.WriteLine("Calling the STT service. Please wait... \n");

            using (var client = new HttpClient())
            {
                using (var fs = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
                {
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    var streamContent = new StreamContent(fs);
                    //streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

                    var response =
                        await client.PostAsync(requestUri, streamContent).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var responseJson = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Response body:\n");
                    Console.WriteLine(responseJson);

                    Console.WriteLine("\nPress any key to exit.");
                    Console.ReadLine();
                }
            }
        }


        private static async Task SimpleSpeechToText(string requestUri, string host, string subscriptionKey,
            string audioFile)
        {
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    using (var fs = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
                    {
                        // Set the HTTP method
                        request.Method = HttpMethod.Post;
                        // Construct the URI
                        request.RequestUri = new Uri(requestUri);
                        // Set the content type header
                        request.Content = new StreamContent(fs);
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

                        // Set additional header, such as Authorization and User-Agent
                        request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                        request.Headers.Add("Connection", "Keep-Alive");
                        // Update your resource name
                        request.Headers.Add("User-Agent", "YOUR_RESOURCE_NAME");
                        // Create a request
                        Console.WriteLine("Calling the STT service. Please wait... \n");
                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            response.EnsureSuccessStatusCode();
                            // Asynchronously read the response
                            var responseJson = await response.Content.ReadAsStringAsync();

                            Console.WriteLine("Response body:\n");
                            Console.WriteLine(responseJson);
                        }

                        Console.WriteLine("\nPress any key to exit.");
                        Console.ReadLine();
                    }
                }
            }
        }

        private static async Task ChunkedSpeechToText(string requestUri, string host, string subscriptionKey,
            string audioFile)
        {
            HttpWebRequest request = null;
            request = (HttpWebRequest) WebRequest.Create(requestUri);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Host = host;
            request.ContentType = @"audio/wav; codecs=audio/pcm; samplerate=16000";
            request.Headers["Ocp-Apim-Subscription-Key"] = subscriptionKey;
            request.AllowWriteStreamBuffering = false;

            using (var fs = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
            {
                /*
                * Open a request stream and write 1024 byte chunks in the stream one at a time.
                */
                byte[] buffer = null;
                var bytesRead = 0;
                using (var requestStream = request.GetRequestStream())
                {
                    /*
                    * Read 1024 raw bytes from the input audio file.
                    */
                    buffer = new byte[checked((uint) Math.Min(1024, (int) fs.Length))];
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                        requestStream.Write(buffer, 0, bytesRead);

                    // Flush
                    requestStream.Flush();
                }
            }

            using (var response = (HttpWebResponse) await request.GetResponseAsync().ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Successfully returned a response. \n");
                    string responseJson = null;

                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var sr = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            responseJson = sr.ReadToEnd();
                        }
                    }

                    Console.WriteLine("Response body:\n");
                    Console.WriteLine(responseJson);
                }
                else
                {
                    Console.WriteLine("Request failed. \n");
                }

                Console.WriteLine("\nPress any key to exit.");
                Console.ReadLine();
            }
        }
    }
}