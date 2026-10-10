-- Tavira · Stats demo seed (PostgreSQL, local DB)
-- Budget "Stats Demo" (EUR): 9 categories (1 soft-deleted) and 15 periods
-- (14 closed, 28-33 days each, + 1 open period that started 9 days ago).
-- Re-runnable: removes the previous "Stats Demo" budget of the same user first.

DO $$
DECLARE
    v_email        text := 'YOUR_EMAIL_HERE';  -- your login email; falls back to the first user
    v_budget_name  text := 'Stats Demo';
    v_closed       int  := 14;                 -- closed periods (+1 open) = 15 total
    v_open_days    int  := 9;                  -- the open period started this many days ago

    v_user_id      int;
    v_currency_id  int;
    v_budget_id    int;
    v_period_id    int;
    v_cat_id       int;
    v_lengths      int[] := '{}';
    v_start        timestamptz;
    v_end          timestamptz;
    v_until        timestamptz;
    v_date         timestamptz;
    v_infl         numeric;
    v_amount       numeric;
    v_n            int;
    i int; j int;
    c record;
BEGIN
    -- user + currency ---------------------------------------------------------
    SELECT "Id" INTO v_user_id FROM "Users" WHERE "Email" = v_email;
    IF v_user_id IS NULL THEN
        SELECT "Id" INTO v_user_id FROM "Users" ORDER BY "Id" LIMIT 1;
    END IF;
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'No users found - log into the app once first.';
    END IF;

    SELECT "Id" INTO v_currency_id FROM "Currencies" WHERE "Code" = 'EUR';

    -- clean previous run ------------------------------------------------------
    DELETE FROM "Spendings" s USING "BudgetCategories" bc, "Budgets" b
     WHERE s."BudgetCategoryId" = bc."Id" AND bc."BudgetId" = b."Id"
       AND b."Name" = v_budget_name AND b."OwnerId" = v_user_id;
    DELETE FROM "BudgetPeriods" p USING "Budgets" b
     WHERE p."BudgetId" = b."Id" AND b."Name" = v_budget_name AND b."OwnerId" = v_user_id;
    DELETE FROM "BudgetCategories" bc USING "Budgets" b
     WHERE bc."BudgetId" = b."Id" AND b."Name" = v_budget_name AND b."OwnerId" = v_user_id;
    DELETE FROM "AppUserBudget" ub USING "Budgets" b
     WHERE ub."BudgetsId" = b."Id" AND b."Name" = v_budget_name AND b."OwnerId" = v_user_id;
    DELETE FROM "Budgets" WHERE "Name" = v_budget_name AND "OwnerId" = v_user_id;

    -- budget ------------------------------------------------------------------
    INSERT INTO "Budgets" ("Name", "CurrencyId", "IsDeleted", "OwnerId")
    VALUES (v_budget_name, v_currency_id, false, v_user_id)
    RETURNING "Id" INTO v_budget_id;

    INSERT INTO "AppUserBudget" ("BudgetsId", "UsersId") VALUES (v_budget_id, v_user_id);

    -- categories: name, min, max, count range per period, deleted?, last period index used, descriptions
    DROP TABLE IF EXISTS demo_cat;
    CREATE TEMP TABLE demo_cat (
        name text, lo numeric, hi numeric, cnt_lo int, cnt_hi int,
        is_deleted bool, last_period int, descs text[], id int);

    INSERT INTO demo_cat VALUES
      ('Rent',          950, 950,  1,  1, false, 99, ARRAY['Monthly rent'], NULL),
      ('Groceries',      18,  95,  8, 14, false, 99, ARRAY['Supermarket','Bakery','Farmers market','Corner shop','Weekly groceries'], NULL),
      ('Eating out',     12,  70,  3,  8, false, 99, ARRAY['Pizza night','Sushi','Coffee & cake','Burger place','Lunch with colleagues'], NULL),
      ('Transport',       5,  60,  4,  9, false, 99, ARRAY['Fuel','Taxi','Metro card','Parking','Car wash'], NULL),
      ('Utilities',      60, 180,  2,  3, false, 99, ARRAY['Electricity','Water','Internet','Phone bill','Heating'], NULL),
      ('Entertainment',  10,  90,  1,  5, false, 99, ARRAY['Cinema','Concert tickets','Streaming','Bowling','Board game'], NULL),
      ('Health',         15, 120,  0,  3, false, 99, ARRAY['Pharmacy','Doctor visit','Vitamins','Dentist check-up'], NULL),
      ('Shopping',       20, 250,  1,  4, false, 99, ARRAY['Clothes','Shoes','Home decor','Electronics accessory','Gift'], NULL),
      ('Gym',            45,  45,  1,  1, true,   5, ARRAY['Gym membership'], NULL);

    FOR c IN SELECT * FROM demo_cat LOOP
        INSERT INTO "BudgetCategories" ("BudgetId", "IsDeleted", "Name")
        VALUES (v_budget_id, c.is_deleted, c.name)
        RETURNING "Id" INTO v_cat_id;
        UPDATE demo_cat SET id = v_cat_id WHERE name = c.name;
    END LOOP;

    -- one-off big purchases (period index is 0-based; v_closed = the open period)
    DROP TABLE IF EXISTS demo_big;
    CREATE TEMP TABLE demo_big (cat text, period_idx int, amount numeric, descr text);
    INSERT INTO demo_big VALUES
      ('Shopping',       3, 1449.99, 'New laptop'),
      ('Transport',      7,  820.00, 'Car service & tyres'),
      ('Entertainment', 10,  640.00, 'Summer holiday flights'),
      ('Health',        12,  560.00, 'Dentist - crown'),
      ('Utilities',     13,  410.00, 'Annual home insurance'),
      ('Shopping',      14,  289.90, 'Winter jacket');

    -- periods -----------------------------------------------------------------
    FOR i IN 1..v_closed LOOP
        v_lengths := v_lengths || (28 + floor(random() * 6))::int;
    END LOOP;

    v_start := date_trunc('day', now())
             - make_interval(days => v_open_days)
             - make_interval(days => (SELECT sum(x) FROM unnest(v_lengths) AS x)::int)
             + interval '8 hours';

    FOR i IN 0..v_closed LOOP
        IF i < v_closed THEN
            v_end   := v_start + make_interval(days => v_lengths[i + 1]);
            v_until := v_end;
        ELSE
            v_end   := NULL;           -- active period
            v_until := now();
        END IF;

        INSERT INTO "BudgetPeriods" ("BudgetId", "StartDate", "EndDate")
        VALUES (v_budget_id, v_start, v_end)
        RETURNING "Id" INTO v_period_id;

        v_infl := 1 + 0.008 * i;       -- ~0.8% price drift per period

        -- regular spendings
        FOR c IN SELECT * FROM demo_cat LOOP
            CONTINUE WHEN i > c.last_period;

            v_n := c.cnt_lo + floor(random() * (c.cnt_hi - c.cnt_lo + 1))::int;
            IF v_end IS NULL AND c.name <> 'Rent' THEN
                v_n := ceil(v_n * 0.3);  -- open period is only a few days old
            END IF;

            FOR j IN 1..v_n LOOP
                IF c.name = 'Rent' THEN
                    v_amount := CASE WHEN i >= 8 THEN 1000 ELSE 950 END;  -- rent increase
                    v_date   := v_start + interval '1 day';
                ELSE
                    v_amount := round((c.lo + random()::numeric * (c.hi - c.lo)) * v_infl, 2);
                    v_date   := v_start + (v_until - v_start) * random();
                END IF;

                INSERT INTO "Spendings"
                    ("Amount", "BudgetCategoryId", "BudgetPeriodId", "CreatedByUserId", "Date", "Description")
                VALUES
                    (-v_amount, c.id, v_period_id, v_user_id, v_date,
                     c.descs[1 + floor(random() * array_length(c.descs, 1))::int]);
            END LOOP;
        END LOOP;

        -- big one-offs for this period
        INSERT INTO "Spendings"
            ("Amount", "BudgetCategoryId", "BudgetPeriodId", "CreatedByUserId", "Date", "Description")
        SELECT -b.amount, dc.id, v_period_id, v_user_id,
               v_start + (v_until - v_start) * random(), b.descr
          FROM demo_big b
          JOIN demo_cat dc ON dc.name = b.cat
         WHERE b.period_idx = i;

        -- funding: positive allocation per category (~10% above spent, rounded up to 50)
        INSERT INTO "Spendings"
            ("Amount", "BudgetCategoryId", "BudgetPeriodId", "CreatedByUserId", "Date", "Description")
        SELECT ceil(-sum(s."Amount") * 1.1 / 50) * 50, s."BudgetCategoryId", v_period_id, v_user_id,
               v_start, 'Budget allocation'
          FROM "Spendings" s
         WHERE s."BudgetPeriodId" = v_period_id AND s."Amount" < 0
         GROUP BY s."BudgetCategoryId";

        IF v_end IS NOT NULL THEN
            v_start := v_end;
        END IF;
    END LOOP;

    DROP TABLE IF EXISTS demo_cat;
    DROP TABLE IF EXISTS demo_big;

    RAISE NOTICE 'Created "%" (budget id %) with % periods for user id %',
        v_budget_name, v_budget_id, v_closed + 1, v_user_id;
END $$;
