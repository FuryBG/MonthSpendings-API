-- Tavira · demo partner for the "Stats Demo" budget (PostgreSQL, local DB only)
-- Run AFTER seed-stats-demo.sql. Adds a fake member "Alex Partner" to the budget and
-- reassigns ~40% of its outgoing spendings to them, so "Who spent" has two people.
-- Re-runnable: reuses the same partner user and re-rolls the split each time.

DO $$
DECLARE
    v_budget_name  text := 'Stats Demo';
    v_partner_mail text := 'demo.partner@tavira.local';
    v_budget_id    int;
    v_owner_id     int;
    v_partner_id   int;
    v_moved        int;
BEGIN
    SELECT "Id", "OwnerId" INTO v_budget_id, v_owner_id
      FROM "Budgets" WHERE "Name" = v_budget_name ORDER BY "Id" DESC LIMIT 1;
    IF v_budget_id IS NULL THEN
        RAISE EXCEPTION 'Budget "%" not found - run seed-stats-demo.sql first.', v_budget_name;
    END IF;

    SELECT "Id" INTO v_partner_id FROM "Users" WHERE "Email" = v_partner_mail;
    IF v_partner_id IS NULL THEN
        INSERT INTO "Users" ("Email", "FirstName", "LastName", "GoogleId", "NotificationToken", "Timezone",
                             "FailedLoginAttempts", "IsPro", "LastVisited", "SyncWalletTransactions")
        VALUES (v_partner_mail, 'Alex', 'Partner', 'demo-partner-google-id', '', 'UTC',
                0, false, now(), false)
        RETURNING "Id" INTO v_partner_id;
    END IF;

    INSERT INTO "AppUserBudget" ("BudgetsId", "UsersId")
    VALUES (v_budget_id, v_partner_id)
    ON CONFLICT DO NOTHING;

    -- Start from a clean split: everything back to the owner, then ~40% to the partner.
    UPDATE "Spendings" s SET "CreatedByUserId" = v_owner_id
      FROM "BudgetCategories" bc
     WHERE s."BudgetCategoryId" = bc."Id" AND bc."BudgetId" = v_budget_id;

    UPDATE "Spendings" s SET "CreatedByUserId" = v_partner_id
      FROM "BudgetCategories" bc
     WHERE s."BudgetCategoryId" = bc."Id" AND bc."BudgetId" = v_budget_id
       AND s."Amount" < 0 AND random() < 0.4;
    GET DIAGNOSTICS v_moved = ROW_COUNT;

    RAISE NOTICE 'Partner user id % added to budget %; % spendings now belong to them.', v_partner_id, v_budget_id, v_moved;
END $$;
