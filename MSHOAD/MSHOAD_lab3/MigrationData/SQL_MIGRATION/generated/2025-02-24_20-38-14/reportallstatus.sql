alter session set current_schema=UNIVERSITY;

set serveroutput on

COLUMN date_time NEW_VAL filename noprint;
SELECT to_char(systimestamp,'yyyy-mm-dd_hh24-mi-ssxff') date_time FROM DUAL;
spool reportallstatus_UNIVERSITY_&filename..log

PROMPT Object Status Report on UNIVERSITY schema
select owner "schema Name", object_name "Object Name", object_type "Object Type", status "Status", timestamp "Created Date and Time" from all_objects where owner = 'UNIVERSITY';
spool off
COMMIT;

alter session set current_schema=EMULATION;

set serveroutput on

COLUMN date_time NEW_VAL filename noprint;
SELECT to_char(systimestamp,'yyyy-mm-dd_hh24-mi-ssxff') date_time FROM DUAL;
spool reportallstatus_EMULATION_&filename..log

PROMPT Object Status Report on EMULATION schema
select owner "schema Name", object_name "Object Name", object_type "Object Type", status "Status", timestamp "Created Date and Time" from all_objects where owner = 'EMULATION';
spool off
COMMIT;

