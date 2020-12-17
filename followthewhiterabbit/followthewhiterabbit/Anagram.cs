using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace followthewhiterabbit
{
    public class Anagram
    {
        // Original phrase 
        private string _originalPhrase { get; set; }
        public string OriginalPhrase { get { return _originalPhrase; } }

        // Hidden messages to be found
        public Dictionary<string, string> HiddenMessages { get; set; }
        public int RabitHoleDepth;

        public Anagram(string originalPhrase)
        {
            this._originalPhrase = originalPhrase;
            this.HiddenMessages = new Dictionary<string, string>();
        }

        public void LoadHiddenMessages(string[] hiddenMessages)
        {
            foreach (var message in hiddenMessages)
                this.HiddenMessages.Add(message, String.Empty);
        }

        public void SaveMessage(string hiddenMessage, string actualMessage)
        {
            if(this.HiddenMessages.Any(hm=>hm.Key == hiddenMessage))
                this.HiddenMessages[hiddenMessage] = actualMessage;
        }

        public string GetMessage (string hiddenMessage)
        {
            if (this.HiddenMessages.Any(hm => hm.Key == hiddenMessage))
                return this.HiddenMessages[hiddenMessage];
            else
                return String.Empty;
        }

        /**
         * Finds an anagram matching the anagramHash using myDictionary
         */
        public bool FindAnagrams(string anagramHash, string[] myDictionary, bool tryHarder = false)
        {
            string input = OriginalPhrase;

            // no. of spaces in input
            RabitHoleDepth = input.Count(c => c.Equals(' '));
            if (tryHarder) RabitHoleDepth++;

            Console.WriteLine("Rabbit hole goes {0} level deep!!", RabitHoleDepth);

            char[] inputChars = input.Replace(" ", String.Empty).Distinct().OrderBy(c => c).ToArray();
            // Lets get the count of all the input characters
            int[] charCounts = new int[inputChars.Length];
            for (var k = 0; k < inputChars.Length; k++)
                charCounts[k] = (byte)input.Count(c => c.Equals(inputChars[k]));

            // Lets get all the valid remaining words
            var remainingWords = myDictionary.Where(s =>
            {
                if (s.Except(inputChars).Any())
                    return false;

                int l = 0;
                foreach (char ch in inputChars)
                {
                    if (s.Count(c => c.Equals(ch)) > charCounts[l])
                        return false;

                    l++;
                }

                return true;

            }).Distinct();
            
            string anagram = String.Empty;

            // Compile the word list into word tree for faster traversal
            WordTree wt = new WordTree();
            foreach (string word in remainingWords)
                wt.CompileWord(word);

            int levels = 0;
            return FindAnagram (anagramHash, input.Replace(" ", String.Empty), wt, wt.Root, anagram, levels);

        }

        public bool FindAnagram (string anagramHash, string inputPhrase, WordTree wordTree, CharNode currentNode, string anagram, int levels)
        {
            string anagramSoFar = anagram;
            string remainingInputPhrase = inputPhrase;
            // See if we can use the character
            var charIndex = inputPhrase.IndexOf(currentNode.Value);
            if (charIndex != -1)
            {
                anagramSoFar += currentNode.Value;
                remainingInputPhrase = inputPhrase.Remove(charIndex, 1);
            }
            else if(currentNode != wordTree.Root)
            {
                // oops this character doesn't exist in phrase, no point in going further to children
                return false;
            }

            // We have covered every character of input phrase
            if (remainingInputPhrase.Length == 0)
            {
                // And we have reached the rabit hole depth, and this current node represents a word
                if(levels == RabitHoleDepth && currentNode.IsWord)
                {
                    Console.Write("\rScanning anagrams: {0} ", anagramSoFar);

                    var computeHash = CreateMD5(anagramSoFar);
                    // See if this is it, if found lets roll
                    if (computeHash.Equals(anagramHash))
                    {
                        Console.WriteLine("matches the hash !!!");
                        this.SaveMessage(anagramHash, anagramSoFar);
                        return true;
                    }
                }

                // keep looking
                return false;
            }

            // Continue looking in children
            foreach (CharNode childNode in currentNode.Children)
            {
                if (FindAnagram(anagramHash, remainingInputPhrase, wordTree, childNode, anagramSoFar, levels))
                    return true;
            }

            // We have a word, lets start over from the root
            if (currentNode.IsWord && levels < RabitHoleDepth)
                if (FindAnagram(anagramHash, remainingInputPhrase, wordTree, wordTree.Root, anagramSoFar + " ", ++levels))
                    return true;
           
            return false;

        }


        public string CreateMD5(string input)
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
