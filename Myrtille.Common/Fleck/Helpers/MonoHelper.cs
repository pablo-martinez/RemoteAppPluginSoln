using System;

namespace Myrtille.Fleck.Helpers
{
  public static class MonoHelper
  {
    public static bool IsRunningOnMono ()
    {
      return Type.GetType ("Mono.Runtime") != null;
    }
  }
}

