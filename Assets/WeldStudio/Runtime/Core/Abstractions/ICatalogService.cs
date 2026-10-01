using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WeldStudio.Core.Data;

namespace WeldStudio.Core
{
    /// <summary>
    /// Every catalog entry known to the application, indexed by id. Holds metadata only: loading the
    /// catalog never loads meshes or textures.
    /// </summary>
    public interface ICatalogService
    {
        bool IsLoaded { get; }

        /// <summary>Valid entries, sorted by <see cref="CatalogItemData.SortOrder"/>.</summary>
        IReadOnlyList<CatalogItemData> Items { get; }

        /// <summary>Raised after entries are added (initial load, content pack, mod).</summary>
        event Action CatalogChanged;

        /// <summary>
        /// Discovers entries by the <see cref="CatalogItemData.CatalogLabel"/> label. Invalid entries and
        /// duplicate ids are skipped with a warning.
        /// </summary>
        Task LoadAsync(CancellationToken cancellationToken = default);

        bool TryGetItem(string id, out CatalogItemData item);

        /// <summary>Equipable entry with <paramref name="id"/>, or null.</summary>
        IEquipableDefinition FindEquipable(string id);
    }
}
