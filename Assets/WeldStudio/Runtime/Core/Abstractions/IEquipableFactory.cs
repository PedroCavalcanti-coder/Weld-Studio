using UnityEngine;
using WeldStudio.Core.Data;

namespace WeldStudio.Core
{
    /// <summary>
    /// Creates the runtime <see cref="IEquipable"/> for a kind of catalog item. Factories are registered in
    /// the DI container, so new kinds of equipables (from modules or the community) plug in without touching
    /// the core.
    /// </summary>
    public interface IEquipableFactory
    {
        bool CanCreate(CatalogItemData definition);

        /// <param name="definition">Item being equipped.</param>
        /// <param name="instance">Freshly instantiated prefab of the item, not yet attached.</param>
        IEquipable Create(CatalogItemData definition, GameObject instance);
    }
}
