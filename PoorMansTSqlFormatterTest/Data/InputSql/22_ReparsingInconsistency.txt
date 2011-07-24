--THIS FILE CONTAINS A KNOWN SQL REFORMATTING INCONSISTENCY:
-- * in the first run, the free-standing comment will not be indented (because it is considered to belong to the second clause)
-- * in the second run, and on after that, the (previously) free-standing comment WILL be indented (because it is considered to belong to the first clause)
-- (this is a small-enough issue that we're not working on it...)

SELECT 1 --a comment attached

--A free standing comment

FROM SomeTable
