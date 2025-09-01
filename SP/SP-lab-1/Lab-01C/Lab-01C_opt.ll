; ModuleID = 'Lab-01C.c'
source_filename = "Lab-01C.c"
target datalayout = "e-m:x-p:32:32-p270:32:32-p271:32:32-p272:64:64-i64:64-i128:128-f80:128-n8:16:32-a:0:32-S32"
target triple = "i686-pc-windows-msvc19.43.34808"

$printf = comdat any

$scanf = comdat any

$__local_stdio_printf_options = comdat any

$__local_stdio_scanf_options = comdat any

$"??_C@_0BE@MNFHGOON@Type?5the?5number?5x?3?5?$AA@" = comdat any

$"??_C@_03DLDNIBIK@?$CFlf?$AA@" = comdat any

$"??_C@_0N@ODIMGOJN@x?$FO2?5?5?$DN?5?$CF?46f?6?$AA@" = comdat any

$"??_C@_0N@MKLHHLBO@x?$FO5?5?5?$DN?5?$CF?46f?6?$AA@" = comdat any

$"??_C@_0N@LPBMJCAN@x?$FO17?5?$DN?5?$CF?46f?6?$AA@" = comdat any

@"??_C@_0BE@MNFHGOON@Type?5the?5number?5x?3?5?$AA@" = linkonce_odr dso_local unnamed_addr constant [20 x i8] c"Type the number x: \00", comdat, align 1
@"??_C@_03DLDNIBIK@?$CFlf?$AA@" = linkonce_odr dso_local unnamed_addr constant [4 x i8] c"%lf\00", comdat, align 1
@"??_C@_0N@ODIMGOJN@x?$FO2?5?5?$DN?5?$CF?46f?6?$AA@" = linkonce_odr dso_local unnamed_addr constant [13 x i8] c"x^2  = %.6f\0A\00", comdat, align 1
@"??_C@_0N@MKLHHLBO@x?$FO5?5?5?$DN?5?$CF?46f?6?$AA@" = linkonce_odr dso_local unnamed_addr constant [13 x i8] c"x^5  = %.6f\0A\00", comdat, align 1
@"??_C@_0N@LPBMJCAN@x?$FO17?5?$DN?5?$CF?46f?6?$AA@" = linkonce_odr dso_local unnamed_addr constant [13 x i8] c"x^17 = %.6f\0A\00", comdat, align 1
@__local_stdio_printf_options._OptionsStorage = internal global i64 0, align 8
@__local_stdio_scanf_options._OptionsStorage = internal global i64 0, align 8

; Function Attrs: nounwind
define dso_local noundef i32 @main() local_unnamed_addr #0 {
  %1 = alloca double, align 8
  call void @llvm.lifetime.start.p0(i64 8, ptr nonnull %1) #6
  %2 = tail call i32 (ptr, ...) @printf(ptr noundef nonnull dereferenceable(1) @"??_C@_0BE@MNFHGOON@Type?5the?5number?5x?3?5?$AA@")
  %3 = call i32 (ptr, ...) @scanf(ptr noundef nonnull @"??_C@_03DLDNIBIK@?$CFlf?$AA@", ptr noundef nonnull %1)
  %4 = load double, ptr %1, align 8
  %5 = fmul double %4, %4
  %6 = fmul double %5, %5
  %7 = fmul double %6, %6
  %8 = fmul double %7, %7
  %9 = fmul double %4, %8
  %10 = fmul double %4, %6
  %11 = call i32 (ptr, ...) @printf(ptr noundef nonnull dereferenceable(1) @"??_C@_0N@ODIMGOJN@x?$FO2?5?5?$DN?5?$CF?46f?6?$AA@", double noundef %5)
  %12 = call i32 (ptr, ...) @printf(ptr noundef nonnull dereferenceable(1) @"??_C@_0N@MKLHHLBO@x?$FO5?5?5?$DN?5?$CF?46f?6?$AA@", double noundef %10)
  %13 = call i32 (ptr, ...) @printf(ptr noundef nonnull dereferenceable(1) @"??_C@_0N@LPBMJCAN@x?$FO17?5?$DN?5?$CF?46f?6?$AA@", double noundef %9)
  call void @llvm.lifetime.end.p0(i64 8, ptr nonnull %1) #6
  ret i32 0
}

; Function Attrs: mustprogress nocallback nofree nosync nounwind willreturn memory(argmem: readwrite)
declare void @llvm.lifetime.start.p0(i64 immarg, ptr nocapture) #1

; Function Attrs: inlinehint nounwind
define linkonce_odr dso_local i32 @printf(ptr noundef %0, ...) local_unnamed_addr #2 comdat {
  %2 = alloca ptr, align 4
  call void @llvm.lifetime.start.p0(i64 4, ptr nonnull %2) #6
  call void @llvm.va_start.p0(ptr nonnull %2)
  %3 = load ptr, ptr %2, align 4
  %4 = call ptr @__acrt_iob_func(i32 noundef 1) #6
  %5 = call ptr @__local_stdio_printf_options()
  %6 = load i64, ptr %5, align 8
  %7 = call i32 @__stdio_common_vfprintf(i64 noundef %6, ptr noundef %4, ptr noundef %0, ptr noundef null, ptr noundef %3) #6
  call void @llvm.va_end.p0(ptr nonnull %2)
  call void @llvm.lifetime.end.p0(i64 4, ptr nonnull %2) #6
  ret i32 %7
}

; Function Attrs: inlinehint nounwind
define linkonce_odr dso_local i32 @scanf(ptr noundef %0, ...) local_unnamed_addr #2 comdat {
  %2 = alloca ptr, align 4
  call void @llvm.lifetime.start.p0(i64 4, ptr nonnull %2) #6
  call void @llvm.va_start.p0(ptr nonnull %2)
  %3 = load ptr, ptr %2, align 4
  %4 = call ptr @__acrt_iob_func(i32 noundef 0) #6
  %5 = call ptr @__local_stdio_scanf_options()
  %6 = load i64, ptr %5, align 8
  %7 = call i32 @__stdio_common_vfscanf(i64 noundef %6, ptr noundef %4, ptr noundef %0, ptr noundef null, ptr noundef %3) #6
  call void @llvm.va_end.p0(ptr nonnull %2)
  call void @llvm.lifetime.end.p0(i64 4, ptr nonnull %2) #6
  ret i32 %7
}

; Function Attrs: mustprogress nocallback nofree nosync nounwind willreturn memory(argmem: readwrite)
declare void @llvm.lifetime.end.p0(i64 immarg, ptr nocapture) #1

; Function Attrs: mustprogress nocallback nofree nosync nounwind willreturn
declare void @llvm.va_start.p0(ptr) #3

; Function Attrs: mustprogress nocallback nofree nosync nounwind willreturn
declare void @llvm.va_end.p0(ptr) #3

; Function Attrs: noinline nounwind
define linkonce_odr dso_local ptr @__local_stdio_printf_options() local_unnamed_addr #4 comdat {
  ret ptr @__local_stdio_printf_options._OptionsStorage
}

declare dso_local ptr @__acrt_iob_func(i32 noundef) local_unnamed_addr #5

declare dso_local i32 @__stdio_common_vfprintf(i64 noundef, ptr noundef, ptr noundef, ptr noundef, ptr noundef) local_unnamed_addr #5

declare dso_local i32 @__stdio_common_vfscanf(i64 noundef, ptr noundef, ptr noundef, ptr noundef, ptr noundef) local_unnamed_addr #5

; Function Attrs: noinline nounwind
define linkonce_odr dso_local ptr @__local_stdio_scanf_options() local_unnamed_addr #4 comdat {
  ret ptr @__local_stdio_scanf_options._OptionsStorage
}

attributes #0 = { nounwind "min-legal-vector-width"="0" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+cmov,+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #1 = { mustprogress nocallback nofree nosync nounwind willreturn memory(argmem: readwrite) }
attributes #2 = { inlinehint nounwind "min-legal-vector-width"="0" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+cmov,+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #3 = { mustprogress nocallback nofree nosync nounwind willreturn }
attributes #4 = { noinline nounwind "min-legal-vector-width"="0" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+cmov,+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #5 = { "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="pentium4" "target-features"="+cmov,+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #6 = { nounwind }

!llvm.module.flags = !{!0, !1, !2}
!llvm.ident = !{!3}

!0 = !{i32 1, !"NumRegisterParameters", i32 0}
!1 = !{i32 1, !"wchar_size", i32 2}
!2 = !{i32 1, !"MaxTLSAlign", i32 65536}
!3 = !{!"clang version 19.1.1"}
