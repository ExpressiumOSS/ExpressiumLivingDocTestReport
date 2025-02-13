using System;
using System.Collections.Generic;

// ------------------------------------------------------------------------------
// This code was generated based on the Cucumber JSON schema
// Changes to this file may cause incorrect behavior and will be lost if 
// the code is regenerated.
// ------------------------------------------------------------------------------

namespace Io.Cucumber.Messages.Types;

/**
 * Represents the PickleTag message in Cucumber's message protocol
 * @see <a href="https://github.com/cucumber/messages" >Github - Cucumber - Messages</a>
 *
 * A tag
 */

public sealed class PickleTag 
{
    public string Name { get; private set; }
    /**
     * Points to the AST node this was created from
     */
    public string AstNodeId { get; private set; }


    public PickleTag(
        string name,
        string astNodeId
    ) 
    {
        RequireNonNull<string>(name, "Name", "PickleTag.Name cannot be null");
        this.Name = name;
        RequireNonNull<string>(astNodeId, "AstNodeId", "PickleTag.AstNodeId cannot be null");
        this.AstNodeId = astNodeId;
    }

    public override bool Equals(Object o) 
    {
        if (this == o) return true;
        if (o == null || this.GetType() != o.GetType()) return false;
        PickleTag that = (PickleTag) o;
        return 
            Name.Equals(that.Name) &&         
            AstNodeId.Equals(that.AstNodeId);        
    }

    public override int GetHashCode() 
    {
        int hash = 17;
        if (Name != null)
          hash = hash * 31 + Name.GetHashCode();
        if (AstNodeId != null)
          hash = hash * 31 + AstNodeId.GetHashCode();
        return hash;
    }

    public override string ToString() 
    {
        return "PickleTag{" +
            "name=" + Name +
            ", astNodeId=" + AstNodeId +
            '}';
    }

    private static T Require<T>(T property, string propertyName, string errorMessage)
    {
      RequireNonNull<T>(property, propertyName, errorMessage);
      return property;
    }
    private static void RequireNonNull<T>(T property, string propertyName, string errorMessage) 
    {
      if (property == null) throw new ArgumentNullException(propertyName, errorMessage);
    }
}
