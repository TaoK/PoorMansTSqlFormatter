SELECT 1
WHERE (((1=1or 10<5)and 20BETWEEN 2and 200)or 10*10=100)
 
DECLARE @Test1 Int, @Test2 Int
SELECT 1
WHERE @Test1 = 1 and @Test2 = 2 and ((1=1and 20BETWEEN 2and 200)or 10*10=100)
 

 