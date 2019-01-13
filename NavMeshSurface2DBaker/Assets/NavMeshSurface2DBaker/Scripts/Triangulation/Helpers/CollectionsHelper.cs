namespace NavMeshSurface2DBaker
{
  public static class CollectionsHelper
  {
    /// <summary>
    /// Wraps a collection index, meaning that an index of size collectionSize+1 would "wrap around" and return a value of 0.
    /// Formula: (index + collectionSize) % collectionSize;
    /// </summary>
    /// <param name="index"></param>
    /// <param name="collectionSize"></param>
    /// <returns>Wrapped index</returns>
    public static int WrapIndex(int index, int collectionSize)
    {
      return (index + collectionSize) % collectionSize;
    }
  }
}
