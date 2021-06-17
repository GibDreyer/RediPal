using System;
using System.Collections.Generic;
using System.Linq;

namespace RedipalCore.Objects
{
    public class RediWriteOptions
    {
        public RediWriteOptions(params string[] setsToAppend)
        {
            SetsToAppend = setsToAppend.ToList();
        }

        /// <summary>
        /// This is the Key space the the object will be wrote under in redis
        /// this is not necessary to set if the attribute "RediKeySpace" is applied to the object class
        /// </summary>
        public string? KeySpace{ get; set; }

        /// <summary>
        /// This is the id the the object will be wrote under in redis the will be located at   {KeySpace}:{ID}.
        /// this is not necessary to set if the attribute "RediWriteName" is applied to a property on the object
        /// </summary>
        public string? ID{ get; set; }

        /// <summary>
        /// If true then the existing object(s) in the given space will be deleted and replaced
        /// </summary>
        public bool DeleteExisting { get; set; }

        /// <summary>
        /// The object being wrote will expire in the given time 
        /// </summary>
        public TimeSpan? Expiration { get; set; }



        /// <summary>
        /// The writer will append the object ID to the sets in this list
        /// </summary>
        public List<string>? SetsToAppend { get; set; }

        /// <summary>
        /// The writer will append the object ID to the sets in this list
        /// </summary>
        public List<string>? SearchableSetsToAppend { get; set; }

        /// <summary>
        /// The writer will remove the id from the given sets 
        /// </summary>
        public List<string>? SetsToRemove { get; set; }
        internal List<string>? Append_ToID { get; set; }

        /// <summary>
        /// The batch that the redis calls will be added to, if not set it will be controlled interanlly
        /// </summary>
        internal RediBatch? RediBatch { get; set; }


        public void AppendToID(string name)
        {
            if (Append_ToID == null)
            {
                Append_ToID = new List<string> { name };
            }
            else
            {
                Append_ToID.Add(name);
            }
        }

        public void AppendToSet(string set, bool isSearchable = false)
        {
            if (isSearchable)
            {
                if (SearchableSetsToAppend == null)
                {
                    SearchableSetsToAppend = new List<string> { set };
                }
                else
                {
                    SearchableSetsToAppend.Add(set);
                }
            }
            else
            {
                if (SetsToAppend == null)
                {
                    SetsToAppend = new List<string> { set };
                }
                else
                {
                    SetsToAppend.Add(set);
                }
            }
        }

        public void RemoveFromSet(string set)
        {
            if (SetsToRemove == null)
            {
                SetsToRemove = new List<string> { set };
            }
            else
            {
                SetsToRemove.Add(set);
            }
        }

        public void SetBatch(RediBatch rediBatch)
        {
            RediBatch = rediBatch;
        }

        public RediBatch? GetBatch()
        {
            return RediBatch;
        }
    }
}
