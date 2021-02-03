# RediPal


    if (hashes.Length > 0)
    {
      var array = new string[hashes.Length];

      for (int i = 0; i < hashes.Length; i++)
      {
          array[i] = hashes[i];
      }
    }
    return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, keySpace.ToLower(), array);
