using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace followthewhiterabbit
{
    class Program
    {
        static int spaceCount;
        static void Main(string[] args)
        {
            string anagramOfPhrase = "poultry outwits ants";

            string easySecretPhraseMD5 = "e4820b45d2277f3844eac66c903e84be";
            string moreDifficultSecretPhraseMD5 = "23170acc097c24edb98fc5488ab033fe";
            string hardSecretPhraseMD5 = "665e5bcb0c20062fe8abaaf4628bb154";

            Stopwatch sw = new Stopwatch();

            sw.Start();
            string easyAnagram = FindAnagrams(anagramOfPhrase, easySecretPhraseMD5);
            sw.Stop();
            Console.WriteLine("Easy Elapsed={0}", sw.Elapsed);

            sw.Restart();
            string moreDifficultAnagram = FindAnagrams(anagramOfPhrase, moreDifficultSecretPhraseMD5);
            sw.Stop();
            Console.WriteLine("More Difficult Elapsed={0}", sw.Elapsed);


            sw.Restart();
            string hardAnagram = FindAnagrams(anagramOfPhrase, hardSecretPhraseMD5);
            sw.Stop();
            Console.WriteLine("Hard Elapsed={0}", sw.Elapsed);


            Console.WriteLine("Easy: {0}", easyAnagram);
            Console.WriteLine("More Difficult: {0}", moreDifficultAnagram);
            Console.WriteLine("Hard: {0}", hardAnagram);

        }

        public static string FindAnagrams(string input, string md5Hash)
        {

            // Lets read the file 
            IEnumerable<string> allWordsFromFile = System.IO.File.ReadAllLines("/Users/ShrutiJha/Harsh/codechallenge/codechallenges/followthewhiterabbit/wordlist");

            int ispaceCount = input.Count(c=>c.Equals(' '));
            spaceCount = ispaceCount;
            // Get all the distinct input characters in sorted manner
            char[] inputChars = input.Replace(" ", "").Distinct().OrderBy(c => c).ToArray();
            // Lets get the count of all the input characters
            byte[] charCounts = new byte [inputChars.Length];
            for (var k = 0; k < inputChars.Length; k++)
                charCounts[k] = (byte)input.Count(c => c.Equals(inputChars[k]));

            // Lets get all the valid remaining words
            var remainingWords = allWordsFromFile.Where(s =>
            {
                if (s.Except(inputChars).Any())
                    return false;
                
                int l = 0;
                foreach (char ch in inputChars)
                {
                    if (s.Count(c => c.Equals(ch)) > (int)charCounts[l])
                        return false;

                    l++;
                }

                return true;

            }).Distinct();

            //IEnumerable<string> validWords = remainingWords.ToArray();

            int lengthOfAnagram = input.Replace(" ", "").Length;
            int remainingLength = lengthOfAnagram;
            ICollection<string> pickedUpWords = new Collection<string>();
            //ICollection<string> anagrams = new Collection<string>();
            int levels = 0;
            return CreateAnagram(remainingWords, inputChars, ref charCounts,
                                ref remainingLength, ref pickedUpWords,
                                ref levels, md5Hash);
        }

        public static string CreateAnagram (IEnumerable<string> remainingWords, char[] inputChars,
                                            ref byte[] charCounts, ref int remainingLength,
                                            ref ICollection<string> pickedUpWords, ref int levels, string md5Hash)
        {

            if (levels > spaceCount)
                return String.Empty;

            // We have nothing further to look for .
            if (remainingLength < 0)
                return String.Empty;

            // We found already
            if (remainingLength == 0)
                return String.Empty;

            IEnumerable<string> validWords = remainingWords.ToList();

            // We still need to find next word, lets look in remaining words;
            foreach (string word in validWords)
            {

                bool checkIfProceed = true;
                char[] excludethesetoo = word.ToCharArray();
                int l = 0;

                foreach (char ch in inputChars)
                {
                    int newCount = excludethesetoo.Count(c => c.Equals(ch));
                    if (newCount > (int)charCounts[l])
                    {
                        checkIfProceed = false;
                        break;
                    }

                    l++;
                }

                // If cannot take the word, letsskip to next
                if (checkIfProceed == false)
                {
                    continue;
                }

                l = 0;
                foreach (char ch in inputChars)
                {
                    int newCount = excludethesetoo.Count(c => c.Equals(ch));

                    if (newCount > 0 && charCounts[l] > 0)
                        charCounts[l] -= (byte)newCount;

                    l++;
                }

                pickedUpWords.Add(word);
                remainingLength -= word.Length;

                byte[] copyCounts = new byte[charCounts.Length];
                copyCounts = charCounts;
                IEnumerable<string> newRemWords = remainingWords.Where(s =>
                {
                    int l = 0;
                    foreach (char ch in inputChars)
                    {
                        if (s.Count(c => c.Equals(ch)) > (int)copyCounts[l])
                            return false;

                        l++;
                    }

                    return true;
                });

                levels++;
                string anag = CreateAnagram(newRemWords, inputChars, ref charCounts,
                                            ref remainingLength, ref pickedUpWords, ref levels, md5Hash);
                if (!anag.Equals(String.Empty))
                {
                    return anag;
                }
                levels--;

                // We have found all the words, lets return
                if (remainingLength == 0)
                {
                    string anagram = String.Join(" ", pickedUpWords);

                    if (CreateMD5(anagram).Equals(md5Hash))
                        return anagram;

                }

                pickedUpWords.Remove(word);
                remainingLength += word.Length;
                l = 0;
                foreach (char ch in inputChars)
                {
                    int newCount = excludethesetoo.Count(c => c.Equals(ch));

                    if (newCount > 0)
                        charCounts[l] += (byte)newCount;

                    l++;
                }
                remainingWords = validWords.AsEnumerable();

            }

            return String.Empty;

        }

        public static string GetAnagram (IEnumerable<string> anagrams, string md5)
        {
            foreach (var anagram in anagrams)
            {
                if (CreateMD5(anagram).Equals(md5))
                    return anagram;
            }

            return String.Empty;
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                md5.Dispose();
                return sb.ToString().ToLower();
            }
        }
    }
}
