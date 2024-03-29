﻿namespace RedipalCore.Objects
{
    public class RediSubscriberOptions
    {
        /// <summary>
        /// Default:  500
        /// </summary>
        public int MaxMembers { get; set; } = 500;
        /// <summary>
        /// Default:  True
        /// </summary>
        public bool SubscribeToSetMembers { get; set; } = true;
        /// <summary>
        /// Default:  False
        /// </summary>
        public bool SubscribeToSubObjects { get; set; } = false;
        /// <summary>
        /// Default:  True
        /// </summary>
        public bool WatchForRemove { get; set; } = true;
        /// <summary>
        /// Default:  True
        /// </summary>
        public bool WatchForAdd { get; set; } = true;
        /// <summary>
        /// Default:  True
        /// </summary>
        public bool DeleteUnloadable { get; set; } = true;
        /// <summary>
        /// Default:  10
        /// </summary>
        public int MaxPublishingInterval { get; set; } = 10;
    }
}