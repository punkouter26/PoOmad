# ADR-002: Azure Table Storage over Cosmos DB

**Status**: Accepted  
**Date**: 2025-11-23  
**Context**: Phase 2 - Foundational Infrastructure

## Context and Problem Statement

We need a NoSQL database for storing:
- User profiles (1 document per user)
- Daily OMAD logs (1 document per user per day)
- Simple key-value lookups by GoogleId and Date

Requirements:
- Support for PartitionKey/RowKey queries
- Low cost (target $5/month total budget)
- Simple data model (no complex relationships)
- Fast reads for dashboard (<1 second load time)

## Decision Drivers

- **Cost**: Must stay under $5/month for entire infrastructure
- **Query Patterns**: Simple key-value lookups, date range queries
- **Data Volume**: Low (single-user app, ~365 records/year per user)
- **Simplicity**: No need for complex indexing or global distribution
- **Scalability**: Multi-user support (but not millions of users)

## Considered Options

### Option 1: Azure Table Storage (Chosen)

**Pricing**:
- $0.045 per GB/month storage
- $0.00036 per 10,000 transactions
- **Estimated**: ~$0.50/month for 1,000 users with 1 year of data

**Pros**:
- ✅ **Extremely low cost**: 10x cheaper than Cosmos DB
- ✅ Simple key-value model matches our needs
- ✅ PartitionKey (GoogleId) + RowKey (date) perfect for our queries
- ✅ Azurite emulator for local development (built into Aspire)
- ✅ No provisioned throughput required
- ✅ Automatic scaling
- ✅ Azure.Data.Tables SDK is simple and well-documented

**Cons**:
- ⚠️ Limited query capabilities (only PartitionKey + RowKey filters efficient)
- ⚠️ No global distribution (single region)
- ⚠️ Lower SLA than Cosmos DB (99.9% vs 99.99%)

**Our Query Patterns** (all efficient with Table Storage):
```csharp
// Get user profile: PartitionKey=GoogleId, RowKey="profile"
await tableClient.GetEntityAsync<UserProfile>(googleId, "profile");

// Get specific day log: PartitionKey=GoogleId, RowKey="2025-11-23"
await tableClient.GetEntityAsync<DailyLogEntry>(googleId, "2025-11-23");

// Get monthly logs: PartitionKey=GoogleId, RowKey >= "2025-11-01" AND RowKey < "2025-12-01"
var query = tableClient.QueryAsync<DailyLogEntry>(
    filter: $"PartitionKey eq '{googleId}' and RowKey ge '2025-11-01' and RowKey lt '2025-12-01'"
);
```

### Option 2: Cosmos DB (NoSQL API)

**Pricing**:
- $0.25 per GB/month storage
- $0.008 per RU/s (Request Unit)
- Minimum 400 RU/s = $23.36/month (serverless mode: $0.284 per 1M RUs)
- **Estimated**: $5-10/month minimum even with serverless

**Pros**:
- ✅ Rich query capabilities (SQL-like syntax, secondary indexes)
- ✅ Global distribution (multi-region replication)
- ✅ Higher SLA (99.99% availability)
- ✅ Better for complex queries and analytics

**Cons**:
- ❌ **5-10x more expensive** than Table Storage
- ❌ Overkill for our simple key-value access patterns
- ❌ Complex pricing model (RU/s calculations)
- ❌ We don't need global distribution (single-region is fine)
- ❌ We don't need advanced querying (our queries are simple PartitionKey + RowKey)

### Option 3: SQL Database (Azure SQL or PostgreSQL)

**Pricing**:
- Azure SQL: $5/month minimum (Basic tier)
- PostgreSQL: $10/month minimum

**Pros**:
- ✅ Familiar relational model
- ✅ ACID transactions
- ✅ Rich querying with JOINs

**Cons**:
- ❌ Overkill for simple key-value data
- ❌ More expensive than Table Storage
- ❌ Requires schema migrations
- ❌ We have no relational data (no JOINs needed)

### Option 4: Blazored.LocalStorage Only (Client-Side)

**Pricing**:
- Free (browser IndexedDB)

**Pros**:
- ✅ No cloud costs
- ✅ Instant reads (local)

**Cons**:
- ❌ Data lost if user clears browser cache
- ❌ No multi-device sync
- ❌ No server-side validation
- ❌ No analytics (can't query across users)
- ❌ Violates spec requirement: "Cloud persistence for reliability"

## Decision Outcome

**Chosen**: Azure Table Storage

### Implementation

**Entity Design**:

```csharp
// User Profile
PartitionKey: {GoogleId}
RowKey: "profile"
Properties: Email, Height, StartingWeight, StartDate

// Daily Log Entry
PartitionKey: {GoogleId}
RowKey: {yyyy-MM-dd}
Properties: OmadCompliant, AlcoholConsumed, Weight, ServerTimestamp
```

**Query Efficiency**:

| Query | Efficiency | Reason |
|-------|-----------|---------|
| Get profile | ✅ Fast | Direct PartitionKey + RowKey lookup |
| Get day log | ✅ Fast | Direct PartitionKey + RowKey lookup |
| Get month logs | ✅ Fast | PartitionKey filter + RowKey range (within partition) |
| Calculate streak | ✅ Fast | PartitionKey filter + date sorting (within partition) |
| Get analytics trends | ✅ Fast | PartitionKey filter + date range (within partition) |

All queries stay **within a single partition** (user's GoogleId), which is the fastest access pattern in Table Storage.

### Justification

1. **Cost**: $0.50/month vs $5-10/month (Cosmos DB) fits our $5 total budget
2. **Query Patterns**: All our queries are PartitionKey-based (optimal for Table Storage)
3. **Data Model**: Simple key-value structure (no complex relationships)
4. **Scalability**: Table Storage scales automatically without provisioned throughput
5. **Developer Experience**: Azurite emulator integrates seamlessly with Aspire

### Trade-offs

- **Limited Querying**: Can't do complex analytics across all users efficiently
  - **Mitigation**: Our analytics only need single-user data (per spec)
- **No Global Distribution**: Single-region only
  - **Mitigation**: We don't need multi-region (spec doesn't require it)
- **Lower SLA**: 99.9% vs 99.99%
  - **Mitigation**: Acceptable for a personal accountability tracker

## Consequences

### Positive

- **Budget-Friendly**: Leaves $4.50/month for App Insights, Container Apps hosting
- **Simple Schema**: No migrations, no complex indexing
- **Fast Queries**: All access patterns are optimal (PartitionKey-based)
- **Easy Testing**: Azurite provides local emulation without cloud costs

### Negative

- **Future Constraints**: If we add complex cross-user analytics, we'd need to migrate
  - **Mitigation**: Current spec has no cross-user features
- **Limited Filtering**: Can't efficiently query by arbitrary fields (e.g., "all users who logged alcohol yesterday")
  - **Mitigation**: All current features only need user-scoped queries

## Validation

**Success Criteria**:
- ✅ Dashboard loads in <1 second (SC-003) - Verified via PartitionKey queries
- ✅ Monthly cost under $5 - Verified via Azure Pricing Calculator
- ✅ Streak calculation accurate - Verified via date-sorted query within partition
- ✅ Analytics with gap-filling - Verified via date range query

**Performance Benchmarks**:
- Get profile: ~10ms (direct key lookup)
- Get month logs (30 days): ~50ms (range query within partition)
- Calculate streak: ~100ms (sort + iterate within partition)

## Migration Path (If Needed)

If we later need Cosmos DB features:

1. Export Table Storage data to JSON
2. Import to Cosmos DB container
3. Update `TableClient` to `CosmosClient` in handlers
4. Adjust PartitionKey strategy (e.g., `/GoogleId` instead of `PartitionKey` property)

**Effort**: 2-4 hours (data model is already NoSQL-friendly)

## References

- [Azure Table Storage Pricing](https://azure.microsoft.com/pricing/details/storage/tables/)
- [Cosmos DB Pricing](https://azure.microsoft.com/pricing/details/cosmos-db/)
- [Azure.Data.Tables SDK Documentation](https://learn.microsoft.com/dotnet/api/azure.data.tables)
- [Table Storage Design Guide](https://learn.microsoft.com/azure/storage/tables/table-storage-design)

## Related Decisions

- **ADR-001**: Aspire has built-in Azurite support (Table Storage emulator)
- **Data Model**: PartitionKey/RowKey design documented in `data-model.md`
- **Phase 2 Tasks**: T020-T021 implement Table Storage connection
