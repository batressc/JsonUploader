-- ============================================================
-- 1. Obtener todas las órdenes filtrando por rango de fechas
-- ============================================================
declare
	@startDate datetime = '2021-04-04 00:00:00',
	@endDate datetime = '2024-04-08 00:00:00';

select 
	dat.Id, dat.OrderType, dat.OrderNumber, dat.SupVersion, jd.jsonValue
from 
	JsonData as dat
	cross apply(select dat.JsonText as '*' for xml path('json'), TYPE) jd(jsonValue)
	cross apply(select json_value(dat.JsonText, '$.requestedCompletionDate') as originalDate) as od
	cross apply(select left(od.originalDate , LEN(od.originalDate) - 2) + ':' + RIGHT(od.originalDate, 2) as normalizedDate) as nd
	cross apply(select switchoffset(nd.normalizedDate, '+00:00') as localDate) ld	
where 
	ISJSON(dat.JsonText) = 1
	and ld.localDate between @startDate and @endDate
GO




-- ============================================================
-- 2. Obtener todos los componentes removidos por orden
-- ============================================================
select 
	orderInfo.fullOrderNumber, prodAction.productCode
from 
	JsonData as dat
	cross apply (select json_value(dat.JsonText, '$.externalId') as fullOrderNumber) as orderInfo
	cross apply openjson(dat.JsonText, '$.orderItem') as ordItemJson
	cross apply (select json_value(ordItemJson.[value], '$.action')) as ordItemAction([action])
	cross apply openjson(ordItemJson.[value], '$.product.characteristic') as ordItemProductCharacteristics
	cross apply (select json_query(ordItemProductCharacteristics.[value], '$.value[0]')) as ordItemProductCharacteristicValue([data])
	cross apply (
		select 
			json_value(ordItemProductCharacteristicValue.[data], '$.action[0]') as [action],
			json_value(ordItemProductCharacteristicValue.[data], '$.productCode[0]') as productCode
	) as productInfo
where 
	isjson(dat.JsonText) = 1
	and ordItemAction.[action] = 'modify'
	and productInfo.[action] = 'delete'
	and productInfo.productCode is not null;
GO




-- ============================================================
-- 6. Obtener todas las órdenes que remuevan dry loop
-- ============================================================
select 
	distinct
	orderInfo.fullOrderNumber
from 
	JsonData as dat
	cross apply (select json_value(dat.JsonText, '$.externalId') as fullOrderNumber) as orderInfo
	cross apply openjson(dat.JsonText, '$.orderItem') as ordItemJson
	cross apply openjson(ordItemJson.[value], '$.product.characteristic') as ordItemProductCharacteristics
	cross apply (select json_query(ordItemProductCharacteristics.[value], '$.value[0]')) as ordItemProductCharacteristicValue([data])
	cross apply (
		select 
			json_value(ordItemProductCharacteristicValue.[data], '$.action[0]') as [action],
			json_value(ordItemProductCharacteristicValue.[data], '$.productCode[0]') as productCode
	) as productInfo
where 
	isjson(dat.JsonText) = 1
	and productInfo.productCode = '132804'
	and productInfo.[action] = 'delete'
GO




-- ============================================================
-- 7. Obtener todas las órdenes que tengan solo un additional STB
-- ============================================================
select 
	orderInfo.fullOrderNumber
from 
	JsonData as dat
	cross apply (select json_value(dat.JsonText, '$.externalId') as fullOrderNumber) as orderInfo
	cross apply openjson(dat.JsonText, '$.orderItem') as ordItemJson
	cross apply openjson(ordItemJson.[value], '$.product.characteristic') as ordItemProductCharacteristics
	cross apply (select json_query(ordItemProductCharacteristics.[value], '$.value[0]')) as ordItemProductCharacteristicValue([data])
	cross apply (select json_value(ordItemProductCharacteristicValue.[data], '$.productCode[0]') as productCode) as productInfo
	cross apply (select case when productInfo.productCode = '920022' then 1 else 0 end as validator) as queryInfo
where 
	isjson(dat.JsonText) = 1
	and productInfo.productCode in ('920086', '920022')
group by 
	orderInfo.fullOrderNumber
having
	sum(queryInfo.validator) = 1
GO




-- ============================================================
-- 8. Obtener todas las órdenes que no tengan additional STB pero tengan STB principal
-- ============================================================
select 
	orderInfo.fullOrderNumber
from 
	JsonData as dat
	cross apply (select json_value(dat.JsonText, '$.externalId') as fullOrderNumber) as orderInfo
	cross apply openjson(dat.JsonText, '$.orderItem') as ordItemJson
	cross apply openjson(ordItemJson.[value], '$.product.characteristic') as ordItemProductCharacteristics
	cross apply (select json_query(ordItemProductCharacteristics.[value], '$.value[0]')) as ordItemProductCharacteristicValue([data])
	cross apply (select json_value(ordItemProductCharacteristicValue.[data], '$.productCode[0]') as productCode) as productInfo
	cross apply (select case when productInfo.productCode = '920022' then 1 else 0 end as validator) as queryInfo
where 
	isjson(dat.JsonText) = 1
	and productInfo.productCode in ('920086', '920022')
group by 
	orderInfo.fullOrderNumber
having
	sum(queryInfo.validator) > 0
GO




-- ============================================================
-- 9. Obtener las órdenes que no tengan promociones
-- ============================================================
select 
	distinct
	orderInfo.fullOrderNumber
from 
	JsonData as dat
	cross apply (select json_value(dat.JsonText, '$.externalId') as fullOrderNumber) as orderInfo
	cross apply openjson(dat.JsonText, '$.orderItem') as ordItemJson
	cross apply openjson(ordItemJson.[value], '$.product.characteristic') as ordItemProductCharacteristics
	cross apply (select json_query(ordItemProductCharacteristics.[value], '$.value[0]')) as ordItemProductCharacteristicValue([data])
	cross apply (select json_value(ordItemProductCharacteristicValue.[data], '$.promoCode[0]') as promoCode) as productInfo
where 
	isjson(dat.JsonText) = 1
	and (
		productInfo.promoCode is null
		or len(productInfo.promoCode) = 0
		or productInfo.promoCode = ''
	)
GO