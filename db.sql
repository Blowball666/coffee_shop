-- coffee_db
-- Таблица ролей
CREATE TABLE Roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL
);
select * from roles;

-- Таблица пользователей
CREATE TABLE Users (
    id SERIAL PRIMARY KEY,
    role_id INT REFERENCES Roles(id),
    last_name VARCHAR(50) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    birth_date DATE,
    phone VARCHAR(15),
    email VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(100) NOT NULL
);
select * from users;


-- Таблица размеров
CREATE TABLE Sizes (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
	volume NUMERIC(5, 2) NOT NULL,
    price_multiplier NUMERIC(10, 2) NOT NULL
);
select * from sizes;

-- Таблица сиропов
CREATE TABLE Syrups (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    price NUMERIC(10, 2) NOT NULL
);
select * from syrups;

-- Таблица ассортимента
CREATE TABLE Assortment (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price NUMERIC(10, 2) NOT NULL,
    can_add_syrup BOOLEAN NOT NULL,
    can_choose_size BOOLEAN NOT NULL,
    in_stock BOOLEAN NOT NULL,
    photo BYTEA
);
select * from assortment;

-- Связь размеров и ассортимента (многие ко многим)
CREATE TABLE Assortment_Sizes (
    size_id INT REFERENCES Sizes(id),
    assortment_id INT REFERENCES Assortment(id),
    PRIMARY KEY (size_id, assortment_id)
);
select * from Assortment_Sizes;

-- Связь сиропов и ассортимента (многие ко многим)
CREATE TABLE Assortment_Syrups (
    syrup_id INT REFERENCES Syrups(id),
    assortment_id INT REFERENCES Assortment(id),
    PRIMARY KEY (syrup_id, assortment_id)
);
select * from Assortment_Syrups;

-- Таблица статусов заказов
CREATE TABLE Statuses (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL
);
select * from statuses;

-- Таблица заказов
CREATE TABLE Orders (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES Users(id),
    order_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status_id INT REFERENCES Statuses(id)
);
select * from Orders;

-- Таблица желаемых товаров
CREATE TABLE Desired_Product (
    id SERIAL PRIMARY KEY,
    assortment_id INT REFERENCES Assortment(id),
    size_id INT REFERENCES Sizes(id),
    quantity INT NOT NULL,
    order_id INT REFERENCES Orders(id),
    item_price NUMERIC(10, 2) NOT NULL
);
select * from Desired_Product;

-- Таблица сиропов в желаемом товаре
CREATE TABLE Desired_Product_Syrups (
    desired_product_id INT REFERENCES Desired_Product(id),
    syrup_id INT REFERENCES Syrups(id),
    quantity INT NOT NULL,
    PRIMARY KEY (desired_product_id, syrup_id)
);
select * from Desired_Product_Syrups;

-- Таблица корзин
CREATE TABLE Carts (
    user_id INT REFERENCES Users(id),
    desired_product_id INT REFERENCES Desired_Product(id),
    PRIMARY KEY (user_id, desired_product_id)
);

-- Таблица отложенных товаров
CREATE TABLE Wishlist (
    user_id INT REFERENCES Users(id),
    assortment_id INT REFERENCES Assortment(id),
    PRIMARY KEY (user_id, assortment_id)
);

-- Таблица истории действий с баллами
CREATE TYPE transaction_type AS ENUM ('earn', 'spend', 'expire');

CREATE TABLE Points_Transactions (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES Users(id),
    order_id INT REFERENCES Orders(id),
    transaction_type transaction_type,
    points INT NOT NULL,
    transaction_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
select * from Points_Transactions;

-- Таблица для связи начислений баллов со списанием и сгоранием 
CREATE TABLE Transaction_Usages (
    id SERIAL PRIMARY KEY,
    usage_transaction_id INT NOT NULL REFERENCES Points_Transactions(id), -- ID списания или сгорания
    earned_transaction_id INT NOT NULL REFERENCES Points_Transactions(id), -- ID начисления
    used_points INT NOT NULL, -- сколько баллов использовано из этого начисления
    UNIQUE (usage_transaction_id, earned_transaction_id)
);
select * from Transaction_Usages;



-- Функция для получения не аннулированных начислений
CREATE OR REPLACE FUNCTION get_user_unexpired_earnings(p_user_id INT)
RETURNS TABLE (
    id INT,
    points INT,
    used_points BIGINT,
    remaining_points BIGINT,
    transaction_date TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        pt.id,
        pt.points,
        COALESCE(SUM(tu.used_points), 0) AS used_points,
        pt.points - COALESCE(SUM(tu.used_points), 0) AS remaining_points,
        pt.transaction_date
    FROM Points_Transactions pt
    LEFT JOIN Transaction_Usages tu ON pt.id = tu.earned_transaction_id
    WHERE pt.user_id = p_user_id
      AND pt.transaction_type = 'earn'
    GROUP BY pt.id, pt.points, pt.transaction_date
    HAVING pt.points > COALESCE(SUM(tu.used_points), 0)
    ORDER BY pt.transaction_date ASC;
END;
$$ LANGUAGE plpgsql;
SELECT * FROM get_user_unexpired_earnings(41);



-- Функция списания баллов
CREATE OR REPLACE FUNCTION spend_points(
    p_user_id INT,
    p_points_to_spend INT,
    p_order_id INT DEFAULT NULL,
    p_type transaction_type DEFAULT 'spend'
)
RETURNS VOID AS $$
DECLARE
    v_remaining INT := p_points_to_spend;
    v_earn RECORD;
    v_used INT;
    v_usage_id INT;
BEGIN
    -- Создаем транзакцию списания или аннулирования
    INSERT INTO Points_Transactions(user_id, order_id, transaction_type, points)
    VALUES (p_user_id, p_order_id, p_type, p_points_to_spend)
    RETURNING id INTO v_usage_id;
    -- Проходим по начислениям
    FOR v_earn IN
        SELECT pt.id, pt.points
        FROM Points_Transactions pt
        LEFT JOIN (
            SELECT earned_transaction_id, SUM(used_points) AS used
            FROM Transaction_Usages
            GROUP BY earned_transaction_id
        ) AS usage ON pt.id = usage.earned_transaction_id
        WHERE pt.user_id = p_user_id
          AND pt.transaction_type = 'earn'
          AND COALESCE(usage.used, 0) < pt.points
        ORDER BY pt.transaction_date ASC
    LOOP
        -- Считаем, сколько можно использовать из этой транзакции
        v_used := LEAST(v_remaining, v_earn.points - COALESCE((SELECT SUM(used_points) 
		FROM Transaction_Usages WHERE earned_transaction_id = v_earn.id), 0));
        EXIT WHEN v_used <= 0;
        -- Вставляем связь использования
        INSERT INTO Transaction_Usages(usage_transaction_id, earned_transaction_id, used_points)
        VALUES (v_usage_id, v_earn.id, v_used);
        v_remaining := v_remaining - v_used;
        EXIT WHEN v_remaining <= 0;
    END LOOP;
    IF v_remaining > 0 THEN
        RAISE EXCEPTION 'Недостаточно баллов для списания';
    END IF;
END;
$$ LANGUAGE plpgsql;
SELECT * FROM spend_points (6, 50);



-- Функция для получения транзакций пользователя о начислении баллов
CREATE OR REPLACE FUNCTION get_user_transactions(p_user_id INT)
RETURNS TABLE (
    id INT,
    user_id INT,
    order_id INT,
    transaction_type transaction_type,
    points INT,
    transaction_date TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        pt.id,
        pt.user_id,
        pt.order_id,
        pt.transaction_type,
        pt.points,
        pt.transaction_date
    FROM Points_Transactions pt
    WHERE pt.user_id = p_user_id
      AND pt.transaction_type = 'earn'
    ORDER BY pt.transaction_date ASC;
END;
$$ LANGUAGE plpgsql;
SELECT * FROM get_user_transactions(6);


-- Представление для вывода итогов по заказам
CREATE VIEW MonthlyOrderReport AS
SELECT 
    EXTRACT(YEAR FROM o.order_date) AS order_year,
    EXTRACT(MONTH FROM o.order_date) AS order_month,
    COUNT(DISTINCT o.id) AS total_orders,
    SUM(dp.quantity) AS total_items_sold,
    SUM(dp.item_price * dp.quantity) AS total_revenue,
    COALESCE(SUM(pt_spend.points), 0) AS total_points_spent,
    COALESCE(SUM(pt_earn.points), 0) AS points_earned
FROM Orders o
LEFT JOIN Desired_Product dp ON o.id = dp.order_id
LEFT JOIN Points_Transactions pt_spend 
    ON o.id = pt_spend.order_id AND pt_spend.transaction_type = 'spend'
LEFT JOIN Points_Transactions pt_earn 
    ON o.id = pt_earn.order_id AND pt_earn.transaction_type = 'earn'
GROUP BY EXTRACT(YEAR FROM o.order_date), EXTRACT(MONTH FROM o.order_date);

SELECT * FROM MonthlyOrderReport ORDER BY order_year DESC, order_month DESC;



-- Представление для получения деталей заказов
CREATE OR REPLACE VIEW OrderDetails AS
SELECT 
    o.id AS order_id,
    u.email AS user_email,
    o.order_date::DATE AS order_date,
    o.order_date::TIME AS order_time,
    dp.item_price * dp.quantity AS total_price,
    dp.quantity,
    pt.points AS points_spent,
    a.name AS assortment_name,
    s.name AS size_name,
    STRING_AGG(DISTINCT sy.name, ', ') AS syrup_names,
    a.price AS price,
    st.name AS status
FROM Orders o
JOIN Users u ON o.user_id = u.id
JOIN Desired_Product dp ON o.id = dp.order_id
JOIN Assortment a ON dp.assortment_id = a.id
LEFT JOIN Sizes s ON dp.size_id = s.id
LEFT JOIN Desired_Product_Syrups dps ON dp.id = dps.desired_product_id
LEFT JOIN Syrups sy ON dps.syrup_id = sy.id
LEFT JOIN Points_Transactions pt ON o.id = pt.order_id AND pt.transaction_type = 'spend'
JOIN Statuses st ON o.status_id = st.id
GROUP BY 
    o.id, u.email, o.order_date::DATE, o.order_date::TIME, 
    dp.item_price, dp.quantity, pt.points, a.name, s.name, a.price, st.name;

select * from OrderDetails;