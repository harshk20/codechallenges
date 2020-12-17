using System;
using System.Diagnostics;
using System.Net;

namespace followthewhiterabbit
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Greatings Trustpilot development team !!");
            Console.WriteLine("I am glad that i took the red pill !!");
            Console.WriteLine("This program is written to find hidden messages !!");

            string anagramOfPhrase = "poultry outwits ants";
            string easySecretPhraseMD5 = "e4820b45d2277f3844eac66c903e84be";
            string moreDifficultSecretPhraseMD5 = "23170acc097c24edb98fc5488ab033fe";
            string hardSecretPhraseMD5 = "665e5bcb0c20062fe8abaaf4628bb154";

            Console.WriteLine("An anagrams of the phrase: {0}!!", anagramOfPhrase);

            Console.WriteLine("Loading the words....");
            if (!System.IO.File.Exists("./wordlist"))
            {
                string url = "https://followthewhiterabbit.trustpilot.com/cs/wordlist";
                Console.WriteLine("Downloading the list from {0}...", url);
                string savePath = @"./wordlist";
                try
                {
                    WebClient client = new WebClient();
                    client.DownloadFile(url, savePath);
                }catch (Exception ex)
                {
                    Console.WriteLine("Oh no!! Something bad happened while downloading the file {0}", ex.Message);
                }
            }
            // Lets read the file 
            string[] myDictionary = System.IO.File.ReadAllLines("./wordlist");
            Console.WriteLine("Dictionary loaded!!");

            string[] hiddenMessageHash = { easySecretPhraseMD5, moreDifficultSecretPhraseMD5, hardSecretPhraseMD5 };

            Anagram anagram = new Anagram(anagramOfPhrase);
            anagram.LoadHiddenMessages(hiddenMessageHash);

            Stopwatch sw = new Stopwatch();
            foreach (var hiddenMessage in hiddenMessageHash)
            {
                Console.WriteLine("Now looking for hidden message with hash: {0} ....", hiddenMessage);

                sw.Restart();
                if (anagram.FindAnagrams(hiddenMessage, myDictionary))
                {
                    Console.WriteLine("*************** Message found !!! ******************* \n Message : {0}", anagram.GetMessage(hiddenMessage));

                }
                else
                {
                    Console.WriteLine("\nMessage not found!! Will try harder again!! ");
                    bool tryHarder = true;
                    if (anagram.FindAnagrams(hiddenMessage, myDictionary, tryHarder))
                    {
                        Console.WriteLine("*************** Message found !!! ******************* \n Message : {0}", anagram.GetMessage(hiddenMessage));
                        Console.WriteLine("************ I see the Matrix now *******************");
                    }

                }

                sw.Stop();
                Console.WriteLine("Time taken : {0}", sw.Elapsed);
                Console.WriteLine("Press any Key to continue finding the next hidden message:");
                Console.ReadKey(true);
            }

            var myhiddenMessage = "4a9f51db2c7eba0c724499f749d3176a";
            Console.WriteLine("This one is for you to find, md5 hash: {0}", myhiddenMessage);

            Console.WriteLine("Find the hidden message and type here:");
            var message = Console.ReadLine().ToLower();

            // Yay you found it
            while(!anagram.CreateMD5(message).Equals(myhiddenMessage))
            {

                Console.WriteLine("Type 'help', if you want to use same program to find the hidden message");
                if (message.Equals("help"))
                {
                    var myAnagram = new Anagram(anagramOfPhrase);
                    string[] hiddenmessages = { myhiddenMessage };
                    myAnagram.LoadHiddenMessages(hiddenmessages);
                    myAnagram.FindAnagrams(myhiddenMessage, myDictionary);

                    message = myAnagram.GetMessage(myhiddenMessage);

                }
                else
                {
                    Console.WriteLine("Find the message and type here:");
                    message = Console.ReadLine().ToLower();
                }
            }

            Console.WriteLine("*************** You found it !!! ******************* \n Message : {0}", message);
            Console.WriteLine("This code challenge has been fun for me.\n I hope you ejoyed my program too!! \n Thank you trustpilot development team!!");
            Console.ReadKey(true);
            Console.Clear();
        }
    }
}
